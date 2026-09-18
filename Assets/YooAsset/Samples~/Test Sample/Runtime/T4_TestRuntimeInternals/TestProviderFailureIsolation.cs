using System;
using System.Collections.Generic;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 验证 Provider 失败收尾时，不会中止其他 Provider 仍在使用的共享加载器。
/// </summary>
/// <remarks>
/// 测试场景：两个资源各自加载不同的主包，但依赖同一个共享依赖包。
/// 测试内容:
/// 1. 两个提供者必须复用同一个共享依赖包加载器实例，且该加载器的引用计数为 2。
/// 2. 资源 B 的主包加载成功后，资源 A 的主包加载失败，此时共享依赖包加载器仍未完成；
///    它的状态应保持 Processing，不能被资源 A 的失败判定为失败。
/// 3. 共享依赖包加载器完成后，资源 B 的句柄应变为 Succeeded，
///    不能沿用资源 A 注入的错误信息。
/// </remarks>
public class TestProviderFailureIsolation
{
    public void RuntimeTester()
    {
        const string expectedError = "Injected failure in main bundle A.";

        using (var scenario = new SharedBundleScenario())
        {
            scenario.StartProviders();

            Assert.AreSame(scenario.SharedLoader, scenario.SharedLoaderForB);
            Assert.AreEqual(2, scenario.SharedReferenceCount);
            Assert.AreEqual(EOperationStatus.Processing, scenario.SharedLoader.Status);
            Assert.AreEqual(EOperationStatus.Processing, scenario.HandleB.Status);

            scenario.CompleteMainBundleB();
            scenario.FailMainBundleA(expectedError);

            Assert.AreEqual(EOperationStatus.Failed, scenario.HandleA.Status);
            Assert.AreEqual(expectedError, scenario.HandleA.Error);

            scenario.UpdateProviderB();
            var sharedStatusAfterFailure = scenario.SharedLoader.Status;
            var providerBStatusAfterFailure = scenario.HandleB.Status;
            scenario.CompleteSharedBundle();
            scenario.UpdateProviderB();

            Assert.AreEqual(EOperationStatus.Succeeded, scenario.HandleB.Status);
            Assert.AreEqual(EOperationStatus.Processing, sharedStatusAfterFailure);
            Assert.AreEqual(EOperationStatus.Processing, providerBStatusAfterFailure);
            Assert.AreEqual(EOperationStatus.Succeeded, scenario.SharedLoader.Status);
        }
    }

    private sealed class SharedBundleScenario : IDisposable
    {
        private const string PackageName = "SharedBundleFailureTest";
        private const int MainBundleAID = 0;
        private const int MainBundleBID = 1;
        private const int SharedBundleID = 2;

        private readonly ResourceManager _manager;
        private readonly PackageBundle _mainBundleA;
        private readonly PackageBundle _mainBundleB;
        private readonly PackageBundle _sharedBundle;
        private readonly BundleFileProvider _providerA;
        private readonly BundleFileProvider _providerB;
        private readonly LoadBundleOperation _mainLoaderA;
        private readonly LoadBundleOperation _mainLoaderB;
        private readonly LoadBundleOperation _sharedLoader;

        public BundleFileHandle HandleA { get; private set; }
        public BundleFileHandle HandleB { get; private set; }
        public LoadBundleOperation SharedLoader => _sharedLoader;
        public LoadBundleOperation SharedLoaderForB { get; }
        public int SharedReferenceCount => _sharedLoader.RefCount;


        public SharedBundleScenario()
        {
            // 构造内存中的资源清单
            var manifest = new PackageManifest();
            _mainBundleA = RuntimeResourceUtility.CreateBundle("main_a");
            _mainBundleB = RuntimeResourceUtility.CreateBundle("main_b");
            _sharedBundle = RuntimeResourceUtility.CreateBundle("shared_x");
            manifest.BundleList.Add(_mainBundleA);
            manifest.BundleList.Add(_mainBundleB);
            manifest.BundleList.Add(_sharedBundle);

            // 创建文件系统宿主
            var host = new FileSystemHost(PackageName);
            host.SetActiveManifest(manifest);
            host.AddFileSystem(new EditorFileSystem());

            // 创建资源管理器
            // 注意：暂停实际加载，由测试注入成功或失败，避免帧时序和 I/O 干扰。
            _manager = new ResourceManager(PackageName);
            RuntimeReflectionUtility.SetField(_manager, "_fileSystemHost", host);
            RuntimeReflectionUtility.SetField(_manager, "_bundleLoadingMaxConcurrency", 0);
            _providerA = CreateProvider("asset_a", MainBundleAID, SharedBundleID);
            _providerB = CreateProvider("asset_b", MainBundleBID, SharedBundleID);
            var loadersA = GetBundleLoaders(_providerA);
            var loadersB = GetBundleLoaders(_providerB);
            _mainLoaderA = loadersA[0];
            _mainLoaderB = loadersB[0];
            _sharedLoader = loadersA[1];
            SharedLoaderForB = loadersB[1];
        }
        public void StartProviders()
        {
            HandleA = _providerA.CreateHandle<BundleFileHandle>();
            HandleB = _providerB.CreateHandle<BundleFileHandle>();
            _providerA.StartOperation();
            _providerB.StartOperation();
            _providerA.UpdateOperation();
            _providerB.UpdateOperation();
        }
        public void FailMainBundleA(string error)
        {
            RuntimeReflectionUtility.InvokeAsyncOperationBaseMethod(_mainLoaderA, "SetError", error);
            _providerA.UpdateOperation();
        }
        public void CompleteMainBundleB()
        {
            CompleteBundle(_mainLoaderB, _mainBundleB);
        }
        public void CompleteSharedBundle()
        {
            if (_sharedLoader.IsDone == false)
                CompleteBundle(_sharedLoader, _sharedBundle);
        }
        public void UpdateProviderB()
        {
            _providerB.UpdateOperation();
        }
        public void Dispose()
        {
            HandleA?.Release();
            HandleB?.Release();

            _providerA.AbortOperation();
            _providerB.AbortOperation();
            _providerA.DestroyProvider();
            _providerB.DestroyProvider();

            Assert.IsTrue(_mainLoaderA.IsReleasable());
            _mainLoaderA.AbortOperation();
            _mainLoaderA.DestroyLoader();

            Assert.IsTrue(_mainLoaderB.IsReleasable());
            _mainLoaderB.AbortOperation();
            _mainLoaderB.DestroyLoader();

            Assert.IsTrue(_sharedLoader.IsReleasable());
            _sharedLoader.AbortOperation();
            _sharedLoader.DestroyLoader();
        }

        private List<LoadBundleOperation> GetBundleLoaders(BundleFileProvider provider)
        {
            var filed = RuntimeReflectionUtility.GetField(provider, "_bundleLoaders");
            return (List<LoadBundleOperation>)filed;
        }
        private BundleFileProvider CreateProvider(string assetName, int mainBundleID, int sharedBundleID)
        {
            var asset = new PackageAsset
            {
                Address = assetName,
                AssetPath = assetName,
                BundleID = mainBundleID,
                DependentBundleIDs = new[] { sharedBundleID }
            };

            var assetInfo = new AssetInfo(PackageName, asset, null);
            return new BundleFileProvider(_manager, assetName, assetInfo);
        }
        private void CompleteBundle(LoadBundleOperation loader, PackageBundle bundle)
        {
            loader.BundleHandle = new RawBundleHandle(bundle, null);
            RuntimeReflectionUtility.InvokeAsyncOperationBaseMethod(loader, "SetResult");
        }
    }
}
