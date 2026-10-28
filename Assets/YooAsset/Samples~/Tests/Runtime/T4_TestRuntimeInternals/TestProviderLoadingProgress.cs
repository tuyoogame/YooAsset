using System;
using System.Collections.Generic;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 在内存中驱动真实加载状态机，验证多依赖、阶段切换和场景激活时的句柄进度。
/// </summary>
public class TestProviderLoadingProgress
{
    public void RuntimeTester()
    {
        var info = new AssetInfo("ProgressTest", "asset");
        TestAssetProvider(new AssetProvider(null, "", info), new ManualAssetLoad(), "_loadAssetOp");
        TestAssetProvider(new AllAssetsProvider(null, "", info), new ManualAllAssetsLoad(), "_loadAllAssetsOp");
        TestAssetProvider(new SubAssetsProvider(null, "", info), new ManualSubAssetsLoad(), "_loadSubAssetsOp");
        TestBundleFileProvider(info);
        TestSceneProvider(info);
        for (int kind = 0; kind < 3; kind++)
        {
            TestFileSystem(kind, false);
            TestFileSystem(kind, true);
        }
        TestCacheForwarding();
        TestWebEncryptedProgress();
        TestBuiltinCopy();
        TestEditorSimulation(0, 1f);
        TestEditorSimulation(1, 1f);
        TestEditorSimulation(4, 0.25f, 0.5f, 0.75f, 1f);
    }

    private static void TestAssetProvider(ProviderBase provider, AsyncOperationBase assetLoad, string field)
    {
        var main = new ManualFileLoad();
        var dependency = new ManualFileLoad();
        PrepareProvider(provider, main, dependency);
        assetLoad.StartOperation();
        SetField(provider, field, assetLoad);

        HandleBase handle;
        float completedProgress = -1f;
        if (provider is AssetProvider)
        {
            var assetHandle = provider.CreateHandle<AssetHandle>();
            assetHandle.Completed += value => completedProgress = value.Progress;
            handle = assetHandle;
        }
        else if (provider is AllAssetsProvider)
        {
            var allHandle = provider.CreateHandle<AllAssetsHandle>();
            allHandle.Completed += value => completedProgress = value.Progress;
            handle = allHandle;
        }
        else
        {
            var subHandle = provider.CreateHandle<SubAssetsHandle>();
            subHandle.Completed += value => completedProgress = value.Progress;
            handle = subHandle;
        }

        Report(main, 0.4f);
        Report(dependency, 0.8f);
        Check(provider, 0.3f);
        Assert.AreEqual(provider.Progress, handle.Progress);
        Report(main, 0f);
        Check(provider, 0.3f);
        Assert.AreEqual(provider.Progress, handle.Progress);
        Report(main, 0.6f);
        Check(provider, 0.35f);
        Succeed(main);
        Check(provider, 0.45f);
        Succeed(dependency);
        Check(provider, 0.5f);

        Report(assetLoad, 0.4f);
        Check(provider, 0.7f);
        Report(assetLoad, 0.8f);
        Check(provider, 0.9f);
        Report(assetLoad, 1f);
        Check(provider, 1f);
        Assert.IsFalse(handle.IsDone);
        Succeed(assetLoad);
        Check(provider, 1f);
        Assert.AreEqual(EOperationStatus.Succeeded, handle.Status);
        Assert.AreEqual(1f, completedProgress);
    }

    private static void TestBundleFileProvider(AssetInfo info)
    {
        var provider = new BundleFileProvider(null, "", info);
        var main = new ManualFileLoad();
        var dependency = new ManualFileLoad();
        PrepareProvider(provider, main, dependency);
        var handle = provider.CreateHandle<BundleFileHandle>();
        float completedProgress = -1f;
        handle.Completed += value => completedProgress = value.Progress;

        Succeed(main);
        Check(provider, 0.5f);
        Report(dependency, 0.6f);
        Check(provider, 0.8f);
        Report(dependency, 0f);
        Check(provider, 0.8f);
        Assert.AreEqual(provider.Progress, handle.Progress);
        Report(dependency, 1f);
        provider.UpdateOperation();
        Assert.AreEqual(1f, handle.Progress);
        Assert.IsFalse(handle.IsDone);
        Succeed(dependency);
        Check(provider, 1f);
        Assert.AreEqual(1f, completedProgress);

        var failed = new BundleFileProvider(null, "", info);
        var failedFile = new ManualFileLoad();
        var pendingDependency = new ManualFileLoad();
        PrepareProvider(failed, failedFile, pendingDependency);
        var failedHandle = failed.CreateHandle<BundleFileHandle>();
        float failedProgress = -1f;
        failedHandle.Completed += value => failedProgress = value.Progress;
        Report(failedFile, 0.4f);
        Report(pendingDependency, 0.2f);
        Check(failed, 0.3f);
        // 主资源包失败时依赖仍未完成，回调中的进度也应归一。
        Fail(failedFile);
        Check(failed, 1f);
        Assert.IsFalse(pendingDependency.IsDone);
        Assert.AreEqual(EOperationStatus.Failed, failedHandle.Status);
        Assert.AreEqual(1f, failedProgress);
    }

    private static void TestSceneProvider(AssetInfo info)
    {
        var provider = new SceneProvider(null, "", info, default, false);
        var file = new ManualFileLoad();
        var scene = new ManualSceneLoad();
        scene.StartOperation();
        PrepareProvider(provider, file);
        SetField(provider, "_loadSceneOp", scene);
        var handle = provider.CreateHandle<SceneHandle>();
        Report(file, 0.6f);
        Check(provider, 0.3f);
        Report(file, 0f);
        Check(provider, 0.3f);
        Assert.AreEqual(provider.Progress, handle.Progress);
        Succeed(file);
        Check(provider, 0.5f);
        Report(scene, 0.45f);
        Check(provider, 0.5f);
        Report(scene, 0.5f);
        Check(provider, 0.5f);
        Report(scene, 0.6f);
        Check(provider, 0.6f);
        Report(scene, 0.8999999f);
        provider.UpdateOperation();
        Assert.Less(handle.Progress, 0.9f);
        Report(scene, 0.9f);
        Check(provider, 0.9f);
        Assert.IsFalse(handle.IsDone);
        Assert.IsFalse(scene.ActivationAllowed);
        handle.AllowSceneActivation();
        provider.UpdateOperation();
        Assert.IsTrue(scene.ActivationAllowed);
        Report(scene, 0.95f);
        Check(provider, 0.95f);
        Succeed(scene);
        Check(provider, 1f);
    }

    private static void TestFileSystem(int kind, bool downloadRequired)
    {
        var bundle = RuntimeResourceUtility.CreateBundle("progress");
        var options = new FSLoadPackageBundleOptions(bundle);
        var cache = new ManualCache();
        var download = new ManualDownload();
        FSLoadPackageBundleOperation operation;
        string downloadField = "_downloadFileOp";
        string downloadStep = "DownloadFile";
        if (kind == 0)
        {
            var fs = new SandboxFileSystem();
            SetField(fs, "<BundleCache>k__BackingField", cache);
            operation = new SFSLoadPackageBundleOperation(fs, options);
        }
        else if (kind == 1)
        {
            var fs = new EditorFileSystem();
            SetField(fs, "<BundleCache>k__BackingField", cache);
            operation = new EFSLoadPackageBundleOperation(fs, options);
        }
        else
        {
            var fs = new BuiltinFileSystem();
            SetField(fs, "<BuiltinBundleCache>k__BackingField", cache);
            SetField(fs, "<UnpackBundleCache>k__BackingField", cache);
            operation = new BFSLoadPackageBundleOperation(fs, options);
            downloadField = "_unpackFileOp";
            downloadStep = "UnpackFile";
        }

        var loader = CreateLoader(operation);
        if (downloadRequired)
        {
            download.StartOperation();
            SetField(operation, downloadField, download);
            Step(operation, downloadStep);
            Report(download, 0.4f);
            Check(loader, 0f);
            Report(download, 0.8f);
            Check(loader, 0f);
            Report(download, 1f);
            Check(loader, 0f);
            Assert.IsFalse(loader.IsDone);
            Succeed(download);
            Check(loader, 0f);
        }
        else
        {
            // 内置直读路径不需要解压；其余文件系统正常执行缓存命中判断。
            if (kind == 2)
                Step(operation, "LoadBundle");
            Check(loader, 0f);
        }

        Report(cache.Load, 0.6f);
        Check(loader, 0.6f);
        Report(cache.Load, 0.8f);
        Check(loader, 0.8f);
        Succeed(cache.Load);
        Check(loader, 1f);
        Assert.AreEqual(EOperationStatus.Succeeded, loader.Status, loader.Error);
    }

    private static void TestCacheForwarding()
    {
        var local = Freeze(new LoadLocalAssetBundleOperation(default));
        var builtin = new BBCLoadAssetBundleOperation(null, null);
        builtin.StartOperation();
        Step(builtin, "LoadBundle");
        SetField(builtin, "_loadLocalAssetBundleOp", local);
        Report(local, 0.6f);
        Check(builtin, 0.6f);
        SetField(local, "<BundleHandle>k__BackingField", new RawBundleHandle(null, null));
        Succeed(local);
        Check(builtin, 1f);

        local = Freeze(new LoadLocalAssetBundleOperation(default));
        var sandbox = new SBCLoadAssetBundleOperation(null, null);
        sandbox.StartOperation();
        Step(sandbox, "LoadBundle");
        SetField(sandbox, "_loadLocalAssetBundleOp", local);
        Report(local, 0.6f);
        Check(sandbox, 0.6f);
        SetField(local, "<BundleHandle>k__BackingField", new RawBundleHandle(null, null));
        Succeed(local);
        Check(sandbox, 1f);
        Assert.AreEqual(EOperationStatus.Succeeded, sandbox.Status);
    }

    private static void TestWebEncryptedProgress()
    {
        var operation = new LoadWebEncryptedAssetBundleOperation(default);
        var request = new ManualRequest();
        operation.StartOperation();
        Step(operation, "CheckRequest");
        SetField(operation, "_downloadBytesRequest", request);
        request.DownloadProgress = 0.6f;
        Check(operation, 0.3f);
        request.DownloadProgress = 1f;
        Check(operation, 0.5f);
        Assert.IsFalse(operation.IsDone);
    }

    private static void TestBuiltinCopy()
    {
        var operation = new CopyBuiltinFileOperation(null, null, null);
        var request = new ManualRequest { DownloadProgress = 0.6f };
        operation.StartOperation();
        Step(operation, "TryCopyByRequest");
        SetField(operation, "_downloadFileRequest", request);
        Check(operation, 0.6f);
        request.DownloadProgress = 0.8f;
        Check(operation, 0.8f);
        request.Done = true;
        Check(operation, 1f);
    }

    private static void TestEditorSimulation(int simulateFrames, params float[] expectedProgress)
    {
        var operation = new ManualEditorLoad();
        // 跳过随机帧数生成，固定帧数以隔离 Unity 随机数接口。
        SetField(operation, "_status", EOperationStatus.Processing);
        Step(operation, "LoadBundle");
        SetField(operation, "_asyncSimulateFrame", simulateFrames);
        SetField(operation, "_totalSimulateFrame", simulateFrames);
        for (int i = 0; i < expectedProgress.Length; i++)
        {
            Check(operation, expectedProgress[i]);
            if (i < expectedProgress.Length - 1)
            {
                Assert.IsFalse(operation.IsDone);
                Assert.AreEqual(EOperationStatus.Processing, operation.Status);
            }
        }
        Assert.IsTrue(operation.IsDone);
        Assert.AreEqual(EOperationStatus.Succeeded, operation.Status);
    }

    private static void PrepareProvider(ProviderBase provider, params ManualFileLoad[] files)
    {
        var loaders = (List<LoadBundleOperation>)RuntimeReflectionUtility.GetField(provider, "_bundleLoaders");
        foreach (var file in files)
            loaders.Add(CreateLoader(file));
        SetField(provider, "_mainBundleLoader", loaders[0]);
        provider.StartOperation();
        Step(provider, "WaitBundleLoader");
    }

    private static LoadBundleOperation CreateLoader(FSLoadPackageBundleOperation operation)
    {
        operation.StartOperation();
        var loader = new LoadBundleOperation(null, null);
        loader.StartOperation();
        Step(loader, "LoadBundleFile");
        SetField(loader, "_loadPackageBundleOp", operation);
        return loader;
    }

    private static T Freeze<T>(T operation) where T : AsyncOperationBase
    {
        operation.StartOperation();
        Step(operation, "None");
        return operation;
    }

    private static void Check(AsyncOperationBase operation, float expected)
    {
        float previous = operation.Progress;
        operation.UpdateOperation();
        Assert.That(operation.Progress, Is.InRange(0f, 1f));
        Assert.GreaterOrEqual(operation.Progress, previous);
        Assert.AreEqual(expected, operation.Progress, 0.00001f, operation.GetType().Name + ": " + operation.Error);
    }

    private static void Step(AsyncOperationBase operation, string name)
    {
        var step = RuntimeReflectionUtility.GetField(operation, "_steps");
        SetField(operation, "_steps", Enum.Parse(step.GetType(), name));
    }

    private static void Report(AsyncOperationBase operation, float value) => SetField(operation, "<Progress>k__BackingField", value);
    private static void SetField(object target, string name, object value) => RuntimeReflectionUtility.SetField(target, name, value);
    private static void Succeed(AsyncOperationBase operation) => RuntimeReflectionUtility.InvokeAsyncOperationBaseMethod(operation, "SetResult");
    private static void Fail(AsyncOperationBase operation) => RuntimeReflectionUtility.InvokeAsyncOperationBaseMethod(operation, "SetError", "Injected load failure.");

    private sealed class ManualFileLoad : FSLoadPackageBundleOperation
    {
        public ManualFileLoad() { BundleHandle = new RawBundleHandle(null, null); }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }
    private sealed class ManualCacheLoad : BCLoadBundleOperation
    {
        public ManualCacheLoad() { BundleHandle = new RawBundleHandle(null, null); }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }
    private sealed class ManualDownload : FSDownloadBundleOperation
    {
        public ManualDownload() : base(null) { }
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }
    private sealed class ManualAssetLoad : BHLoadAssetOperation
    {
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }
    private sealed class ManualAllAssetsLoad : BHLoadAllAssetsOperation
    {
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }
    private sealed class ManualSubAssetsLoad : BHLoadSubAssetsOperation
    {
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
    }
    private sealed class ManualSceneLoad : BHLoadSceneOperation
    {
        public bool ActivationAllowed;
        protected override void InternalStart() { }
        protected override void InternalUpdate() { }
        protected override void InternalAllowSceneActivation() { ActivationAllowed = true; }
    }
    private sealed class ManualEditorLoad : EBCLoadBundleBaseOperation
    {
        public ManualEditorLoad() : base(null, null) { }
        protected override void CreateBundleHandle() { SetResult(); }
    }
    private sealed class ManualCache : IBundleCache
    {
        public readonly ManualCacheLoad Load = new ManualCacheLoad();
        public string PackageName => "ProgressTest";
        public string RootPath => "";
        public bool IsReadOnly => true;
        public int FileCount => 1;
        public long SpaceOccupied => 0;
        public bool IsCached(string guid) => true;
        public BCLoadBundleOperation LoadBundleAsync(BCLoadBundleOptions options) => Load;
        public void Dispose() { }
        public string GetCacheFilePath(string guid) => throw new NotSupportedException();
        public BCInitializeOperation InitializeAsync() => throw new NotSupportedException();
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options) => throw new NotSupportedException();
        public BCClearCacheOperation ClearCacheAsync(BCClearCacheOptions options) => throw new NotSupportedException();
        public BCVerifyCacheOperation VerifyCacheAsync(BCVerifyCacheOptions options) => throw new NotSupportedException();
    }
    private sealed class ManualRequest : IDownloadBytesRequest, IDownloadFileRequest
    {
        public bool Done;
        public float DownloadProgress { get; set; }
        public string Url => "memory://progress";
        public bool IsDone => Done;
        public EDownloadRequestStatus Status => EDownloadRequestStatus.Succeeded;
        public long DownloadedBytes => 0;
        public long HttpCode => 200;
        public string HttpError => "";
        public string Error => "";
        public string SavePath => "";
        byte[] IDownloadBytesRequest.Result => null;
        public void SendRequest() { }
        public void AbortRequest() { }
        public void Dispose() { }
    }
}

