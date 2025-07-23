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

public class T3_TestCacheFileSystem : IPrebuildSetup, IPostBuildCleanup
{
    public void Setup()
    {
    }
    public void Cleanup()
    {
    }

    [UnityTest]
    public IEnumerator A_InitializePackage()
    {
        // 清空旧的缓存目录
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string cacheRoot = $"{projectPath}/yoo";
        Directory.Delete(cacheRoot, true);

        // 拷贝打包资源到本地服务器
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(T2_TestBuldinFileSystem.ASSET_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            string testServerDirectory = "C://xampp/htdocs/CDN/Android/Test";
            CopyDirectory(packageRoot, testServerDirectory);
        }

        // 初始化资源包 ASSET_BUNDLE
        {
            var package = YooAssets.CreatePackage(TestDefine.AssetBundlePackageName);

            // 初始化资源包
            var initParams = new HostPlayModeParameters();
            var fileDecryption = new TestFileStreamDecryption();
            var manifestServices = new TestRestoreManifest();

            string hostServerIP = "http://127.0.0.1/CDN/Android/Test/";
            var remoteServices = new TestRemoteServices(hostServerIP);
            initParams.BuildinFileSystemParameters = null;
            initParams.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, fileDecryption);
            initParams.CacheFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestServices);
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
    private class TestRemoteServices : IRemoteServices
    {
        private readonly string _localServerRoot;

        public TestRemoteServices(string localServerRoot)
        {
            _localServerRoot = localServerRoot;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_localServerRoot}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_localServerRoot}/{fileName}";
        }
    }

    [UnityTest]
    public IEnumerator C1_TestBundlePlaying()
    {
        var tester = new TestBundlePlaying();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C2_TestBundleImporter()
    {
        var tester = new TestBundleImporter();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C3_TestBundleDownloader()
    {
        var tester = new TestBundleDownloader();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator D_DestroyPackage()
    {
        var tester = new TestDestroyPackage();
        yield return tester.RuntimeTester(false);
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        // 检查源目录是否存在
        if (!Directory.Exists(sourceDir))
        {
            throw new DirectoryNotFoundException($"源目录不存在: {sourceDir}");
        }

        // 创建目标目录（如果不存在）
        Directory.CreateDirectory(targetDir);

        // 拷贝所有文件
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(targetDir, fileName);
            File.Copy(file, destFile, true); // true 表示覆盖已存在文件
        }

        // 递归拷贝子目录
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string newTargetDir = Path.Combine(targetDir, dirName);
            CopyDirectory(subDir, newTargetDir);
        }
    }
}