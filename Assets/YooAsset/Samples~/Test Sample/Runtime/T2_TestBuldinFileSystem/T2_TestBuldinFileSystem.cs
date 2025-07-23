using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

public class T2_TestBuldinFileSystem : IPrebuildSetup, IPostBuildCleanup
{
    public const string ASSET_BUNDLE_PACKAGE_ROOT_KEY = "T2_ASSET_BUNDLE_PACKAGE_ROOT_KEY";
    public const string RAW_BUNDLE_PACKAGE_ROOT_KEY = "T2_RAW_BUNDLE_PACKAGE_ROOT_KEY";

    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        // 构建AssetBundlePackage
        {
            var buildParams = new PackageInvokeBuildParam(TestDefine.AssetBundlePackageName);
            buildParams.BuildPipelineName = "ScriptableBuildPipeline";
            buildParams.InvokeAssmeblyName = "YooAsset.Test.Editor";
            buildParams.InvokeClassFullName = "TestPackageBuilder";
            buildParams.InvokeMethodName = "BuildPackage";
            var simulateResult = PackageInvokeBuilder.InvokeBuilder(buildParams);
            UnityEditor.EditorPrefs.SetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }

        // 构建RawBundlePackage
        {
            var buildParams = new PackageInvokeBuildParam(TestDefine.RawBundlePackageName);
            buildParams.BuildPipelineName = "RawFileBuildPipeline";
            buildParams.InvokeAssmeblyName = "YooAsset.Test.Editor";
            buildParams.InvokeClassFullName = "TestPackageBuilder";
            buildParams.InvokeMethodName = "BuildPackage";
            var simulateResult = PackageInvokeBuilder.InvokeBuilder(buildParams);
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

            var package = YooAssets.CreatePackage(TestDefine.AssetBundlePackageName);

            // 初始化资源包
            var initParams = new OfflinePlayModeParameters();
            var fileDecryption = new TestFileStreamDecryption();
            var manifestServices = new TestRestoreManifest();
            initParams.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(fileDecryption, packageRoot);
            initParams.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.DISABLE_CATALOG_FILE, true);
            initParams.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestServices);
            var initializeOp = package.InitializeAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeed)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, initializeOp.Status);

            // 请求资源版本
            var requetVersionOp = package.RequestPackageVersionAsync();
            yield return requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
                Debug.LogError(requetVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, requetVersionOp.Status);

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            yield return updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
                Debug.LogError(updateManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, updateManifestOp.Status);
        }

        // 初始化资源包 RAW_BUNDLE
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(RAW_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestDefine.RawBundlePackageName);

            // 初始化资源包
            var initParams = new OfflinePlayModeParameters();
            initParams.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(null, packageRoot);
            initParams.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
            initParams.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.DISABLE_CATALOG_FILE, true);
            var initializeOp = package.InitializeAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeed)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, initializeOp.Status);

            // 请求资源版本
            var requetVersionOp = package.RequestPackageVersionAsync();
            yield return requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
                Debug.LogError(requetVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, requetVersionOp.Status);

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            yield return updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
                Debug.LogError(updateManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, updateManifestOp.Status);
        }
    }

    [UnityTest]
    public IEnumerator B1_TestAsyncTask()
    {
        var tester = new TestAsyncTask();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B2_TestLoadAsset()
    {
        var tester = new TestLoadAsset();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B3_TestLoadSubAssets()
    {
        var tester = new TestLoadSubAssets();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B4_TestLoadAllAssets()
    {
        var tester = new TestLoadAllAssets();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B5_TestLoadSpriteAtlas()
    {
        var tester = new TestLoadSpriteAtlas();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B6_TestLoadScriptableObject()
    {
        var tester = new TestLoadScriptableObject();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B7_TestLoadScene()
    {
        var tester = new TestLoadScene();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B8_TestLoadRawFile()
    {
        var tester = new TestLoadRawFile();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B9_TestLoadVideo()
    {
        var tester = new TestLoadVideo();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C1_TestGetAssetInfos()
    {
        var tester = new TestGetAssetInfos();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C2_TestBundleEncryption()
    {
        var tester = new TestBundleEncryption();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C3_TestBundleUnpacker()
    {
        var tester = new TestBundleUnpacker();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C4_TestBundleReference()
    {
        var tester = new TestBundleReference();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C4_TestBundleUnload()
    {
        var tester = new TestBundleUnload();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D_DestroyPackage()
    {
        var tester = new TestDestroyPackage();
        yield return tester.RuntimeTester(true);
    }
}