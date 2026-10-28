using System;
using System.Collections.Generic;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 在内存中驱动初始化操作，验证进度传递、阶段折算和衔接，不依赖资源构建或网络。
/// </summary>
public class TestInitializationProgress
{
    public void RuntimeTester()
    {
        TestPackageProgress(new EditorSimulateModeOptions(), 1);
        TestPackageProgress(new OfflinePlayModeOptions(), 1);
        TestPackageProgress(new HostPlayModeOptions(), 2);
        TestPackageProgress(new WebPlayModeOptions(), 2);
        TestPackageProgress(new CustomPlayModeOptions(), 3);
        TestSandboxCacheProgress();
        TestBuiltinFileSystemProgress(false);
        TestBuiltinFileSystemProgress(true);
        TestInvalidFileSystemParameters();
    }

    private static void TestPackageProgress(InitializePackageOptions options, int count)
    {
        var children = new ManualFileSystemOperation[count];
        var parameters = new FileSystemParameters[count];
        for (int i = 0; i < count; i++)
        {
            children[i] = new ManualFileSystemOperation();
            parameters[i] = new FileSystemParameters(typeof(ProgressFileSystem).AssemblyQualifiedName, null);
            parameters[i].AddParameter("Operation", children[i]);
        }

        if (options is EditorSimulateModeOptions editor)
            editor.EditorFileSystemParameters = parameters[0];
        else if (options is OfflinePlayModeOptions offline)
            offline.BuiltinFileSystemParameters = parameters[0];
        else if (options is HostPlayModeOptions host)
        {
            host.BuiltinFileSystemParameters = parameters[0];
            host.CacheFileSystemParameters = parameters[1];
        }
        else if (options is WebPlayModeOptions web)
        {
            web.WebServerFileSystemParameters = parameters[0];
            web.WebNetworkFileSystemParameters = parameters[1];
        }
        else if (options is CustomPlayModeOptions custom)
            custom.FileSystemParameterList.AddRange(parameters);

        var package = new ResourcePackage("InitializationProgressTest");
        var operation = new InitializePackageOperation(package, options);
        try
        {
            operation.StartOperation();
            float previous = 0f;
            for (int i = 0; i < count; i++)
            {
                CheckProgress(operation, i / (float)count, ref previous);
                Assert.IsFalse(operation.IsDone);

                // 初始化开始后，调用方修改原列表不能改变进度分母或待执行的文件系统。
                if (options is CustomPlayModeOptions customOptions)
                    customOptions.FileSystemParameterList.Clear();

                children[i].Report(0.4f);
                CheckProgress(operation, (i + 0.4f) / count, ref previous);
                children[i].Report(0.2f);
                CheckProgress(operation, (i + 0.2f) / count, ref previous, checkMonotonic: false);
                children[i].Report(0.8f);
                CheckProgress(operation, (i + 0.8f) / count, ref previous);
                children[i].Succeed();
                CheckProgress(operation, (i + 1f) / count, ref previous);
            }

            CheckProgress(operation, 1f, ref previous);
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
        }
        finally
        {
            operation.AbortOperation();
            package.InternalDestroy();
        }
    }

    private static void TestSandboxCacheProgress()
    {
        // 冻结实际子任务的状态机，仅注入进度和完成状态，隔离磁盘扫描与校验线程。
        var search = Freeze(new SearchCacheFilesOperation(null));
        var verify = Freeze(new VerifyCacheFilesOperation(null, EFileVerifyLevel.Low, 1, new List<SearchFileInfo>()));
        var operation = new SBCInitializeOperation(null);
        RuntimeReflectionUtility.SetField(operation, "_searchCacheFilesOp", search);
        RuntimeReflectionUtility.SetField(operation, "_verifyCacheFilesOp", verify);
        operation.StartOperation();

        float previous = 0f;
        Report(search, 0.4f);
        CheckProgress(operation, 0.2f, ref previous);
        Succeed(search);
        CheckProgress(operation, 0.5f, ref previous);
        Assert.IsFalse(operation.IsDone);
        Report(verify, 0.5f);
        CheckProgress(operation, 0.75f, ref previous);
        Succeed(verify);
        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
    }

    private static void TestBuiltinFileSystemProgress(bool copyManifest)
    {
        var fileSystem = new BuiltinFileSystem();
        fileSystem.SetParameter(nameof(EFileSystemParameter.CopyBuiltinPackageManifest), copyManifest);
        // 使用未启动的调度器占位，避免本测试向全局调度器注册任务。
        fileSystem.UnpackScheduler = new DownloadSchedulerOperation(default(DownloadSchedulerOperation.Configuration));
        var copy = Freeze(new CopyBuiltinPackageManifestOperation(null));
        var builtin = new ManualCacheOperation();
        var unpack = new ManualCacheOperation();
        builtin.StartOperation();
        unpack.StartOperation();
        var operation = new BFSInitializeOperation(fileSystem);
        RuntimeReflectionUtility.SetField(operation, "_copyBuiltinPackageManifestOp", copy);
        RuntimeReflectionUtility.SetField(operation, "_initializeBuiltinBundleCacheOp", builtin);
        RuntimeReflectionUtility.SetField(operation, "_initializeUnpackBundleCacheOp", unpack);
        operation.StartOperation();
        SetStep(operation, "CopyPackageManifest");

        float previous = 0f;
        if (copyManifest)
        {
            Report(copy, 0.6f);
            CheckProgress(operation, 0f, ref previous);
            Succeed(copy);
        }
        CheckProgress(operation, 0f, ref previous);
        builtin.Report(0.5f);
        CheckProgress(operation, 0.25f, ref previous);
        builtin.Succeed();
        CheckProgress(operation, 0.5f, ref previous);
        Assert.IsFalse(operation.IsDone);
        unpack.Report(0.5f);
        CheckProgress(operation, 0.75f, ref previous);
        unpack.Succeed();
        CheckProgress(operation, 1f, ref previous);
        Assert.AreEqual(EOperationStatus.Succeeded, operation.Status, operation.Error);
    }

    private static void TestInvalidFileSystemParameters()
    {
        var lists = new[] { null, new List<FileSystemParameters>(), new List<FileSystemParameters> { null } };
        foreach (var list in lists)
        {
            var operation = new InitializeFileSystemOperation(new FileSystemHost("InvalidProgressTest"), list);
            operation.StartOperation();
            float previous = 0f;
            CheckProgress(operation, 1f, ref previous);
            Assert.AreEqual(EOperationStatus.Failed, operation.Status);
            Assert.IsNotEmpty(operation.Error);
        }
    }

    private static void CheckProgress(AsyncOperationBase operation, float expected, ref float previous, bool checkMonotonic = true)
    {
        operation.UpdateOperation();
        Assert.That(operation.Progress, Is.InRange(0f, 1f));
        if (checkMonotonic)
            Assert.GreaterOrEqual(operation.Progress, previous, operation.GetType().Name);
        Assert.AreEqual(expected, operation.Progress, 0.00001f, operation.Error);
        previous = operation.Progress;
    }

    private static T Freeze<T>(T operation) where T : AsyncOperationBase
    {
        operation.StartOperation();
        SetStep(operation, "None");
        return operation;
    }

    private static void SetStep(AsyncOperationBase operation, string step)
    {
        var current = RuntimeReflectionUtility.GetField(operation, "_steps");
        RuntimeReflectionUtility.SetField(operation, "_steps", Enum.Parse(current.GetType(), step));
    }

    private static void Report(AsyncOperationBase operation, float progress)
    {
        RuntimeReflectionUtility.SetField(operation, "<Progress>k__BackingField", progress);
    }

    private static void Succeed(AsyncOperationBase operation)
    {
        RuntimeReflectionUtility.InvokeAsyncOperationBaseMethod(operation, "SetResult");
    }

    private sealed class ManualFileSystemOperation : FSInitializeOperation
    {
        public void Report(float progress) { Progress = progress; }
        public void Succeed() { SetResult(); }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }

    private sealed class ManualCacheOperation : BCInitializeOperation
    {
        public void Report(float progress) { Progress = progress; }
        public void Succeed() { SetResult(); }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }

    private sealed class ProgressFileSystem : IFileSystem
    {
        private FSInitializeOperation _operation;
        public string PackageName { get; private set; }
        public FSInitializeOperation InitializeAsync() => _operation;
        public void SetParameter(string name, object value) { _operation = (FSInitializeOperation)value; }
        public void OnCreate(string packageName, string root) { PackageName = packageName; }
        public void OnDestroy() { }
        public bool CanAcceptBundle(PackageBundle bundle) => false;
        public bool IsDownloadRequired(PackageBundle bundle) => false;
        public bool IsUnpackRequired(PackageBundle bundle) => false;
        public bool IsImportRequired(PackageBundle bundle) => false;
        public FSRequestPackageVersionOperation RequestPackageVersionAsync(FSRequestPackageVersionOptions options) => throw new NotSupportedException();
        public FSLoadPackageManifestOperation LoadPackageManifestAsync(FSLoadPackageManifestOptions options) => throw new NotSupportedException();
        public FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options) => throw new NotSupportedException();
        public FSEnsurePackageBundleOperation EnsurePackageBundleAsync(FSEnsurePackageBundleOptions options) => throw new NotSupportedException();
        public FSDownloadBundleOperation DownloadBundleAsync(FSDownloadBundleOptions options) => throw new NotSupportedException();
        public FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options) => throw new NotSupportedException();
    }
}
