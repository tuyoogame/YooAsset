using System;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 验证缓存清理的进度传递、多个文件系统之间的阶段折算和失败收尾。
/// </summary>
public class TestClearCacheProgress
{
    public void RuntimeTester()
    {
        TestStages(1);
        TestStages(2);
        TestStages(3);
        TestForwarding(new BFSClearCacheOperation(null, default(FSClearCacheOptions)));
        TestForwarding(new SFSClearCacheOperation(null, default(FSClearCacheOptions)));
        TestForwarding(new EFSClearCacheOperation(null, default(FSClearCacheOptions)));
        TestImmediateStage();
        TestFailure();
        TestEmptyFileSystems();
    }

    private static void TestStages(int count)
    {
        var host = new FileSystemHost("ClearCacheProgressTest");
        var children = new ManualClearOperation[count];
        for (int i = 0; i < count; i++)
        {
            children[i] = new ManualClearOperation();
            host.AddFileSystem(new ProgressFileSystem(children[i]));
        }

        var operation = new ClearCacheOperation(host, new ClearCacheOptions("ClearAllBundleFiles"));
        operation.StartOperation();
        float previous = 0f;
        CheckProgress(operation, 0f, ref previous);

        // 初始化快照之后新增文件系统，不应影响本次清理的分母和待执行列表。
        var addedLater = new ManualClearOperation();
        host.AddFileSystem(new ProgressFileSystem(addedLater));
        for (int i = 0; i < count; i++)
        {
            CheckProgress(operation, i / (float)count, ref previous);
            Assert.IsFalse(operation.IsDone);
            children[i].Report(0.6f);
            CheckProgress(operation, (i + 0.6f) / count, ref previous);
            children[i].Report(0.2f);
            CheckProgress(operation, (i + 0.6f) / count, ref previous);
            children[i].Succeed();
            CheckProgress(operation, (i + 1f) / count, ref previous);
        }

        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
        Assert.AreEqual(EOperationStatus.None, addedLater.Status);
    }

    private static void TestForwarding(FSClearCacheOperation fileSystemOperation)
    {
        // 注入内存中的缓存清理任务，避免实际删除文件。
        var cacheOperation = new ManualCacheClearOperation();
        cacheOperation.StartOperation();
        fileSystemOperation.StartOperation();
        var step = RuntimeReflectionUtility.GetField(fileSystemOperation, "_steps");
        RuntimeReflectionUtility.SetField(fileSystemOperation, "_steps", Enum.Parse(step.GetType(), "ClearCache"));
        RuntimeReflectionUtility.SetField(fileSystemOperation, "_clearCacheOp", cacheOperation);
        var operation = CreateOperation(fileSystemOperation);

        float previous = 0f;
        cacheOperation.Report(0.25f);
        CheckProgress(operation, 0.25f, ref previous);
        cacheOperation.Report(0.75f);
        CheckProgress(operation, 0.75f, ref previous);
        cacheOperation.Succeed();
        CheckProgress(operation, 1f, ref previous);
        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
    }

    private static void TestImmediateStage()
    {
        var pending = new ManualClearOperation();
        var operation = CreateOperation(new FSClearCacheCompleteOperation(), pending);
        float previous = 0f;
        CheckProgress(operation, 0.5f, ref previous);
        CheckProgress(operation, 0.5f, ref previous);
        Assert.IsFalse(operation.IsDone);
        pending.Report(0.5f);
        CheckProgress(operation, 0.75f, ref previous);
        pending.Succeed();
        CheckProgress(operation, 1f, ref previous);
        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
    }

    private static void TestFailure()
    {
        var failed = new ManualClearOperation();
        var pending = new ManualClearOperation();
        var operation = CreateOperation(failed, pending);
        float previous = 0f;
        failed.Report(0.6f);
        CheckProgress(operation, 0.3f, ref previous);
        failed.Fail("Injected cache clear failure.");
        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Failed, operation.Status);
        Assert.AreEqual("Injected cache clear failure.", operation.Error);
        Assert.AreEqual(EOperationStatus.None, pending.Status);
    }

    private static void TestEmptyFileSystems()
    {
        var operation = CreateOperation();
        float previous = 0f;
        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Failed, operation.Status);
        Assert.IsNotEmpty(operation.Error);
    }

    private static ClearCacheOperation CreateOperation(params FSClearCacheOperation[] children)
    {
        var host = new FileSystemHost("ClearCacheProgressTest");
        foreach (var child in children)
            host.AddFileSystem(new ProgressFileSystem(child));
        var operation = new ClearCacheOperation(host, new ClearCacheOptions("ClearAllBundleFiles"));
        operation.StartOperation();
        return operation;
    }

    private static void CheckProgress(AsyncOperationBase operation, float expected, ref float previous)
    {
        operation.UpdateOperation();
        Assert.That(operation.Progress, Is.InRange(0f, 1f));
        Assert.GreaterOrEqual(operation.Progress, previous);
        Assert.AreEqual(expected, operation.Progress, 0.00001f, operation.Error);
        previous = operation.Progress;
    }

    private sealed class ManualClearOperation : FSClearCacheOperation
    {
        public void Report(float progress) { Progress = progress; }
        public void Succeed() { SetResult(); }
        public void Fail(string error) { SetError(error); }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }

    private sealed class ManualCacheClearOperation : BCClearCacheOperation
    {
        public void Report(float progress) { Progress = progress; }
        public void Succeed() { SetResult(); }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }

    private sealed class ProgressFileSystem : IFileSystem
    {
        private readonly FSClearCacheOperation _operation;
        public ProgressFileSystem(FSClearCacheOperation operation) { _operation = operation; }
        public string PackageName => "ClearCacheProgressTest";
        public FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options) => _operation;
        public void SetParameter(string name, object value) { }
        public void OnCreate(string packageName, string root) { }
        public void OnDestroy() { }
        public bool CanAcceptBundle(PackageBundle bundle) => false;
        public bool IsDownloadRequired(PackageBundle bundle) => false;
        public bool IsUnpackRequired(PackageBundle bundle) => false;
        public bool IsImportRequired(PackageBundle bundle) => false;
        public FSInitializeOperation InitializeAsync() => throw new NotSupportedException();
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options) => throw new NotSupportedException();
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options) => throw new NotSupportedException();
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options) => throw new NotSupportedException();
        public FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options) => throw new NotSupportedException();
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options) => throw new NotSupportedException();
    }
}
