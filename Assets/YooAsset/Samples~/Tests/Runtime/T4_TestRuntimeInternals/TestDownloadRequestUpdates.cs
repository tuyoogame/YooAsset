using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEngine.Networking;
using YooAsset;

/// <summary>
/// 验证请求显式更新、纯状态查询，以及解压链路中的异步转同步。
/// </summary>
public class TestDownloadRequestUpdates
{
    public void RuntimeTester()
    {
        TestPureUnityRequestQuery();
        TestSimulatedRequestUpdates();
        TestDisposedRequests();
        TestCopyAndUnpack(false);
        TestCopyAndUnpack(true);
    }

    private static void TestPureUnityRequestQuery()
    {
        using (var request = new ProbeRequest("file:///unused"))
        {
            foreach (EDownloadRequestStatus status in Enum.GetValues(typeof(EDownloadRequestStatus)))
            {
                request.SetStatus(status);
                for (int i = 0; i < 3; i++)
                {
                    Assert.AreEqual(status == EDownloadRequestStatus.Succeeded || status == EDownloadRequestStatus.Failed,
                        request.IsDone);
                    Assert.AreEqual(status, request.Status);
                    Assert.AreEqual(0, request.DownloadedBytes);
                    Assert.AreEqual(0, request.SucceededCount);
                }
            }
        }
    }

    private static void TestSimulatedRequestUpdates()
    {
        using (var request = new SimulatedFileRequest(new SimulatedDownloadRequestArgs("memory://test", 100, 10)))
        {
            request.UpdateRequest();
            Assert.AreEqual(EDownloadRequestStatus.None, request.Status);
            request.SendRequest();
            RuntimeReflectionUtility.SetField(request, "_lastUpdateTime", TimeUtility.RealtimeSinceStartup - 1d);
            for (int i = 0; i < 3; i++)
            {
                Assert.IsFalse(request.IsDone);
                Assert.AreEqual(0, request.DownloadedBytes);
                Assert.AreEqual(0f, request.DownloadProgress);
            }

            request.UpdateRequest();
            Assert.GreaterOrEqual(request.DownloadedBytes, 10);
            Assert.IsFalse(request.IsDone);

            // 不等待下一帧，第二次显式更新仍能推进并完成请求。
            RuntimeReflectionUtility.SetField(request, "_lastUpdateTime", TimeUtility.RealtimeSinceStartup - 20d);
            request.UpdateRequest();
            Assert.IsTrue(request.IsDone);
            Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status);
            Assert.AreEqual(100, request.DownloadedBytes);
            Assert.AreEqual(1f, request.DownloadProgress);
            request.UpdateRequest();
            Assert.AreEqual(100, request.DownloadedBytes);
        }

        using (var request = new SimulatedFileRequest(new SimulatedDownloadRequestArgs("memory://canceled", 100, 10)))
        {
            request.SendRequest();
            request.AbortRequest();
            request.UpdateRequest();
            Assert.IsTrue(request.IsDone);
            Assert.AreEqual(EDownloadRequestStatus.Failed, request.Status);
            Assert.AreEqual(0, request.DownloadedBytes);
        }
    }

    private static void TestDisposedRequests()
    {
        using (var request = new ProbeRequest("file:///unused"))
        {
            request.Dispose();
            request.SendRequest();
            request.UpdateRequest();
            Assert.AreEqual(0, request.CreatedCount);
        }
        using (var request = new ProbeRequest("file:///unused"))
        {
            request.SetStatus(EDownloadRequestStatus.Running);
            request.Dispose();
            Assert.DoesNotThrow(request.UpdateRequest);
            Assert.DoesNotThrow(request.AbortRequest);
        }
        using (var request = new SimulatedFileRequest(new SimulatedDownloadRequestArgs("memory://disposed", 100, 10)))
        {
            request.SendRequest();
            RuntimeReflectionUtility.SetField(request, "_lastUpdateTime", TimeUtility.RealtimeSinceStartup - 20d);
            request.Dispose();
            request.UpdateRequest();
            Assert.AreEqual(0, request.DownloadedBytes);
        }
    }

    private static void TestCopyAndUnpack(bool waitSynchronously)
    {
        var fileSystem = new BuiltinFileSystem();
        RuntimeReflectionUtility.SetField(fileSystem, "_tempFilesRoot",
            Path.Combine(Path.GetTempPath(), "YooAssetRequestTest-" + Guid.NewGuid().ToString("N")));
        var bundle = new PackageBundle { FileHash = "test-bundle", FileSize = 3 };
        var request = new StepFileRequest();
        var copy = new CopyBuiltinFileOperation(fileSystem, "unused", "unused");
        var unpack = new UnpackAndCacheFileOperation(fileSystem, bundle, "unused");
        var write = new BCWriteCacheCompleteOperation();
        try
        {
            request.SendRequest();
            copy.StartOperation();
            SetStep(copy, "TryCopyByRequest");
            RuntimeReflectionUtility.SetField(copy, "_downloadFileRequest", request);
            unpack.StartOperation();
            SetStep(unpack, "CopyLocalFile");
            RuntimeReflectionUtility.SetField(unpack, "_copyBuiltinFileOp", copy);
            write.StartOperation();
            RuntimeReflectionUtility.SetField(unpack, "_writeCacheOp", write);

            // 先异步执行一步，验证报告使用本次更新后的进度。
            unpack.UpdateOperation();
            Assert.AreEqual(1, request.UpdateCount);
            Assert.AreEqual(1, copy.CopiedBytes);
            Assert.AreEqual(1, unpack.LatestReport.DownloadedBytes);
            Assert.AreEqual(1f / 3f, unpack.LatestReport.DownloadProgress, 0.00001f);
            Assert.IsFalse(unpack.IsDone);

            if (waitSynchronously)
                unpack.WaitForCompletion();
            else
            {
                unpack.UpdateOperation();
                unpack.UpdateOperation();
            }

            Assert.AreEqual(EOperationStatus.Succeeded, copy.Status, copy.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, unpack.Status, unpack.Error);
            Assert.AreEqual(3, request.UpdateCount);
            Assert.AreEqual(3, unpack.LatestReport.DownloadedBytes);
            Assert.AreEqual(1f, unpack.LatestReport.DownloadProgress);
            Assert.IsTrue(request.Disposed);
        }
        finally
        {
            unpack.AbortOperation();
            copy.AbortOperation();
            write.AbortOperation();
            request.Dispose();
        }
    }

    public void LocalUnityRequestTester()
    {
        string path = Path.Combine(Path.GetTempPath(), "YooAssetRequestTest-" + Guid.NewGuid().ToString("N"));
        byte[] data = { 1, 2, 3, 4 };
        try
        {
            File.WriteAllBytes(path, data);
            using (var request = new ProbeRequest(new Uri(path).AbsoluteUri))
            {
                request.UpdateRequest();
                Assert.AreEqual(EDownloadRequestStatus.None, request.Status);
                request.SendRequest();
                var timer = Stopwatch.StartNew();
                while (!request.NativeRequest.isDone && timer.ElapsedMilliseconds < 5000)
                    Thread.Sleep(1);
                Assert.IsTrue(request.NativeRequest.isDone, "Local UnityWebRequest did not finish within 5 seconds.");

                // 引擎请求已完成，封装层仍须显式更新；读取 IsDone 不提取结果或释放资源。
                Assert.IsFalse(request.IsDone);
                Assert.IsFalse(request.IsDone);
                Assert.AreEqual(EDownloadRequestStatus.Running, request.Status);
                Assert.AreEqual(0, request.SucceededCount);
                Assert.IsNull(request.Data);
                request.UpdateRequest();
                Assert.AreEqual(EDownloadRequestStatus.Succeeded, request.Status, request.Error);
                CollectionAssert.AreEqual(data, request.Data);
                Assert.AreEqual(data.Length, request.DownloadedBytes);
                Assert.IsTrue(request.IsDone);
                request.UpdateRequest();
                request.Dispose();
                request.UpdateRequest();
                Assert.AreEqual(1, request.SucceededCount);
                CollectionAssert.AreEqual(data, request.Data);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void SetStep(AsyncOperationBase operation, string name)
    {
        var step = RuntimeReflectionUtility.GetField(operation, "_steps");
        RuntimeReflectionUtility.SetField(operation, "_steps", Enum.Parse(step.GetType(), name));
    }

    private sealed class ProbeRequest : UnityWebRequestBase
    {
        public int CreatedCount;
        public int SucceededCount;
        public byte[] Data;
        public UnityWebRequest NativeRequest;

        public ProbeRequest(string url) : base(new DownloadRequestArgs(url, 5, 0), null) { }
        public void SetStatus(EDownloadRequestStatus status) { Status = status; }
        protected override UnityWebRequest CreateWebRequest()
        {
            CreatedCount++;
            NativeRequest = UnityWebRequest.Get(Url);
            return NativeRequest;
        }
        protected override void OnRequestSucceeded(UnityWebRequest webRequest)
        {
            SucceededCount++;
            Data = webRequest.downloadHandler.data;
        }
    }

    private sealed class StepFileRequest : IDownloadFileRequest
    {
        public int UpdateCount;
        public bool Disposed;
        public string Url => "memory://copy";
        public string SavePath => "unused";
        public EDownloadRequestStatus Status { get; private set; }
        public bool IsDone => Status == EDownloadRequestStatus.Succeeded || Status == EDownloadRequestStatus.Failed;
        public float DownloadProgress => UpdateCount / 3f;
        public long DownloadedBytes => UpdateCount;
        public long HttpCode => 200;
        public string HttpError => null;
        public string Error => null;
        public void SendRequest() { Status = EDownloadRequestStatus.Running; }
        public void UpdateRequest()
        {
            if (Disposed || Status != EDownloadRequestStatus.Running)
                return;
            if (++UpdateCount == 3)
                Status = EDownloadRequestStatus.Succeeded;
        }
        public void AbortRequest() { Status = EDownloadRequestStatus.Failed; }
        public void Dispose() { Disposed = true; }
    }
}
