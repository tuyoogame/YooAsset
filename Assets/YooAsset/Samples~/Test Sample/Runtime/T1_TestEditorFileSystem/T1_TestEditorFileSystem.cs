using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// EditorSimulate 模式测试套件
/// </summary>
/// <remarks>
/// 使用 EditorSimulateBuildPipeline 构建虚拟资源包，通过 EditorSimulateModeOptions 初始化。
/// 覆盖基础加载、信息查询、句柄释放、全量卸载等 CommonTests 公共用例。
/// </remarks>
public class T1_TestEditorFileSystem : IPrebuildSetup, IPostBuildCleanup
{
    private const string ASSET_BUNDLE_PACKAGE_ROOT_KEY = "T1_ASSET_BUNDLE_PACKAGE_ROOT_KEY";
    private const string RAW_BUNDLE_PACKAGE_ROOT_KEY = "T1_RAW_BUNDLE_PACKAGE_ROOT_KEY";
    private const string ARCHIVE_BUNDLE_PACKAGE_ROOT_KEY = "T1_ARCHIVE_BUNDLE_PACKAGE_ROOT_KEY";

    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        // 构建 AssetBundlePackage
        {
            var simulateParams = new PackageBuildParameters(TestConsts.AssetBundlePackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.BuildBundleType = (int)EBundleType.VirtualAssetBundle;
            simulateParams.AssemblyName = "YooAsset.Tests.Editor";
            simulateParams.TypeFullName = "TestPackageBuilder";
            simulateParams.MethodName = "BuildPackage";
            var simulateResult = PackageBuildInvoker.InvokeBuild(simulateParams);
            UnityEditor.EditorPrefs.SetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }

        // 构建 RawBundlePackage
        {
            var simulateParams = new PackageBuildParameters(TestConsts.RawBundlePackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.BuildBundleType = (int)EBundleType.VirtualRawBundle;
            simulateParams.AssemblyName = "YooAsset.Tests.Editor";
            simulateParams.TypeFullName = "TestPackageBuilder";
            simulateParams.MethodName = "BuildPackage";
            var simulateResult = PackageBuildInvoker.InvokeBuild(simulateParams);
            UnityEditor.EditorPrefs.SetString(RAW_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }

        // 构建 ArchiveBundlePackage
        {
            var simulateParams = new PackageBuildParameters(TestConsts.ArchiveBundlePackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.BuildBundleType = (int)EBundleType.VirtualArchiveBundle;
            simulateParams.AssemblyName = "YooAsset.Tests.Editor";
            simulateParams.TypeFullName = "TestPackageBuilder";
            simulateParams.MethodName = "BuildPackage";
            var simulateResult = PackageBuildInvoker.InvokeBuild(simulateParams);
            UnityEditor.EditorPrefs.SetString(ARCHIVE_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }
#endif
    }
    void IPostBuildCleanup.Cleanup()
    {
    }


    [UnityTest]
    public IEnumerator A_InitializePackage()
    {
        // 初始化 AssetBundlePackage
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestConsts.AssetBundlePackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeOptions();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializePackageAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, initializeOp.Status);

            // 请求资源版本
            var requestVersionOp = package.RequestPackageVersionAsync();
            yield return requestVersionOp;
            if (requestVersionOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(requestVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, requestVersionOp.Status);

            // 更新资源清单
            var loadPackageManifestOptions = new LoadPackageManifestOptions(requestVersionOp.PackageVersion, 60);
            var loadPackageManifestOp = package.LoadPackageManifestAsync(loadPackageManifestOptions);
            yield return loadPackageManifestOp;
            if (loadPackageManifestOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(loadPackageManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, loadPackageManifestOp.Status);
        }

        // 初始化 RawBundlePackage
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(RAW_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestConsts.RawBundlePackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeOptions();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializePackageAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, initializeOp.Status);

            // 请求资源版本
            var requestVersionOp = package.RequestPackageVersionAsync();
            yield return requestVersionOp;
            if (requestVersionOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(requestVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, requestVersionOp.Status);

            // 更新资源清单
            var loadPackageManifestOptions = new LoadPackageManifestOptions(requestVersionOp.PackageVersion, 60);
            var loadPackageManifestOp = package.LoadPackageManifestAsync(loadPackageManifestOptions);
            yield return loadPackageManifestOp;
            if (loadPackageManifestOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(loadPackageManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, loadPackageManifestOp.Status);
        }

        // 初始化 ArchiveBundlePackage
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(ARCHIVE_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestConsts.ArchiveBundlePackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeOptions();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializePackageAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, initializeOp.Status);

            // 请求资源版本
            var requestVersionOp = package.RequestPackageVersionAsync();
            yield return requestVersionOp;
            if (requestVersionOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(requestVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, requestVersionOp.Status);

            // 更新资源清单
            var loadPackageManifestOptions = new LoadPackageManifestOptions(requestVersionOp.PackageVersion, 60);
            var loadPackageManifestOp = package.LoadPackageManifestAsync(loadPackageManifestOptions);
            yield return loadPackageManifestOp;
            if (loadPackageManifestOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(loadPackageManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, loadPackageManifestOp.Status);
        }
    }

    [UnityTest]
    public IEnumerator B01_TestAsyncTask()
    {
        var tester = new TestAsyncTask();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B02_TestAsyncCompleted()
    {
        var tester = new TestAsyncCompleted();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B03_TestLoadAsset()
    {
        var tester = new TestLoadAsset();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B04_TestLoadSubAssets()
    {
        var tester = new TestLoadSubAssets();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B05_TestLoadAllAssets()
    {
        var tester = new TestLoadAllAssets();
        yield return tester.RuntimeTester();
    }
    
    [UnityTest]
    public IEnumerator B06_TestLoadGameObject()
    {
        var tester = new TestLoadGameObject();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B07_TestLoadSpriteAtlas()
    {
        var tester = new TestLoadSpriteAtlas();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B08_TestLoadScriptableObject()
    {
        var tester = new TestLoadScriptableObject();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B09_TestLoadScene()
    {
        var tester = new TestLoadScene();
        yield return tester.RuntimeTester();
    }
    
    [UnityTest]
    public IEnumerator B10_TestLoadBundleFile()
    {
        var tester = new TestLoadBundleFile();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B11_TestLoadRawBundle()
    {
        var tester = new TestLoadRawBundle();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B12_TestLoadArchiveBundle()
    {
        var tester = new TestLoadArchiveBundle();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B13_TestEnsureBundleFile_RawBundle()
    {
        var tester = new TestEnsureBundleFile();
        yield return tester.RuntimeTester_RawBundle();
    }

    [UnityTest]
    public IEnumerator B14_TestEnsureBundleFile_AssetBundle()
    {
        var tester = new TestEnsureBundleFile();
        yield return tester.RuntimeTester_AssetBundle();
    }

    [UnityTest]
    public IEnumerator B15_TestEnsureBundleFile_ArchiveBundle()
    {
        var tester = new TestEnsureBundleFile();
        yield return tester.RuntimeTester_ArchiveBundle();
    }

    [UnityTest]
    public IEnumerator B16_TestUniTask()
    {
        var tester = new TestUniTask();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C01_TestGetPackageInfo()
    {
        var tester = new TestGetPackageInfo();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C02_TestGetAssetInfo()
    {
        var tester = new TestGetAssetInfo();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C03_TestIsLocationValid()
    {
        var tester = new TestIsLocationValid();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C04_TestLoadInvalidAsset()
    {
        var tester = new TestLoadInvalidAsset();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C05_TestDuplicateLoad()
    {
        var tester = new TestDuplicateLoad();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C06_TestHandleRelease()
    {
        var tester = new TestHandleRelease();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C07_TestBundleFileRelease()
    {
        var tester = new TestBundleFileRelease();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C08_TestUnloadAllAssets()
    {
        var tester = new TestUnloadAllAssets();
        yield return tester.RuntimeTester();
    }
    
    [UnityTest]
    public IEnumerator Z_DestroyPackage()
    {
        var tester = new TestDestroyPackage();
        yield return tester.RuntimeTester(true, true);
    }
}