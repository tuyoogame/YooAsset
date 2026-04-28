using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// Builtin（离线）模式测试套件
/// </summary>
/// <remarks>
/// 使用 ScriptableBuildPipeline / RawFileBuildPipeline 构建真实资源包，通过 OfflinePlayModeOptions 初始化。
/// 在 CommonTests 基础上额外覆盖加密加载、内置文件解压、引用完整性、精确卸载等 T2 专属用例。
/// </remarks>
public class T2_TestBuiltinFileSystem : IPrebuildSetup, IPostBuildCleanup
{
    public const string ASSET_BUNDLE_PACKAGE_ROOT_KEY = "T2_ASSET_BUNDLE_PACKAGE_ROOT_KEY";
    public const string RAW_BUNDLE_PACKAGE_ROOT_KEY = "T2_RAW_BUNDLE_PACKAGE_ROOT_KEY";

    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        // 构建AssetBundlePackage
        {
            var buildParams = new PackageBuildParameters(TestConsts.AssetBundlePackageName);
            buildParams.BuildPipelineName = "ScriptableBuildPipeline";
            buildParams.AssemblyName = "YooAsset.Tests.Editor";
            buildParams.TypeFullName = "TestPackageBuilder";
            buildParams.MethodName = "BuildPackage";
            var simulateResult = PackageBuildInvoker.InvokeBuild(buildParams);
            UnityEditor.EditorPrefs.SetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }

        // 构建RawBundlePackage
        {
            var buildParams = new PackageBuildParameters(TestConsts.RawBundlePackageName);
            buildParams.BuildPipelineName = "RawFileBuildPipeline";
            buildParams.AssemblyName = "YooAsset.Tests.Editor";
            buildParams.TypeFullName = "TestPackageBuilder";
            buildParams.MethodName = "BuildPackage";
            var simulateResult = PackageBuildInvoker.InvokeBuild(buildParams);
            UnityEditor.EditorPrefs.SetString(RAW_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }
#endif
    }
    void IPostBuildCleanup.Cleanup()
    {
    }


    [UnityTest]
    public IEnumerator A_InitializePackage()
    {
        // 初始化资源包 ASSET_BUNDLE
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestConsts.AssetBundlePackageName);

            // 初始化资源包
            var initParams = new OfflinePlayModeOptions();
            var manifestServices = new TestManifestDecryptor();
            initParams.BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(packageRoot);
            initParams.BuiltinFileSystemParameters.AddParameter(EFileSystemParameter.ManifestDecryptor, manifestServices);
            initParams.BuiltinFileSystemParameters.AddParameter(EFileSystemParameter.AssetbundleDecryptor, new TestFileStreamDecryption());
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

        // 初始化资源包 RAW_BUNDLE
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(RAW_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestConsts.RawBundlePackageName);

            // 初始化资源包
            var initParams = new OfflinePlayModeOptions();
            initParams.BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(packageRoot);
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
    public IEnumerator B10_TestLoadRawFile()
    {
        var tester = new TestLoadRawFile();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B11_TestUniTask()
    {
        var tester = new TestUniTask();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C01_TestBundleEncryption()
    {
        var tester = new TestBundleEncryption();
        yield return tester.RuntimeTester();
    }
    
    [UnityTest]
    public IEnumerator C02_TestResourceUnpacker()
    {
        var tester = new TestResourceUnpacker();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C03_TestBundleReference()
    {
        var tester = new TestBundleReference();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C04_TestBundleUnload()
    {
        var tester = new TestBundleUnload();
        yield return tester.RuntimeTester();
    }


    [UnityTest]
    public IEnumerator D01_TestGetPackageInfo()
    {
        var tester = new TestGetPackageInfo();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D02_TestGetAssetInfo()
    {
        var tester = new TestGetAssetInfo();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D03_TestIsLocationValid()
    {
        var tester = new TestIsLocationValid();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D04_TestLoadInvalidAsset()
    {
        var tester = new TestLoadInvalidAsset();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D05_TestDuplicateLoad()
    {
        var tester = new TestDuplicateLoad();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D06_TestHandleRelease()
    {
        var tester = new TestHandleRelease();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D07_TestRawFileRelease()
    {
        var tester = new TestRawFileRelease();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D08_TestUnloadAllAssets()
    {
        var tester = new TestUnloadAllAssets();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator Z_DestroyPackage()
    {
        var tester = new TestDestroyPackage();
        yield return tester.RuntimeTester(true);
    }
}