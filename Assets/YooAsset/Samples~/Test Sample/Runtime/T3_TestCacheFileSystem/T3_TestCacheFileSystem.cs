using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// Cache（联机）模式测试套件
/// </summary>
/// <remarks>
/// 复用 T2 构建产物，拷贝到本地 HTTP 服务器目录，通过 HostPlayModeOptions 初始化。
/// 覆盖边玩边下、资源导入、资源下载、缓存清理、清单清理等 T3 专属用例。
/// 前置依赖: 需要本地 HTTP 服务器（见 TestConsts.TestServerURL）。
/// </remarks>
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
        if (Directory.Exists(cacheRoot))
            Directory.Delete(cacheRoot, true);

        // 拷贝打包资源到本地服务器
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(T2_TestBuiltinFileSystem.ASSET_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            CopyDirectory(packageRoot, TestConsts.TestServerDirectory);
        }

        // 初始化资源包 ASSET_BUNDLE
        {
            var package = YooAssets.CreatePackage(TestConsts.AssetBundlePackageName);

            // 初始化资源包
            var initParams = new HostPlayModeOptions();
            var manifestServices = new TestManifestDecryptor();

            var remoteService = new TestRemoteService(TestConsts.TestServerURL);
            initParams.BuiltinFileSystemParameters = null;
            initParams.CacheFileSystemParameters = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteService);
            initParams.CacheFileSystemParameters.AddParameter(EFileSystemParameter.ManifestDecryptor, manifestServices);
            initParams.CacheFileSystemParameters.AddParameter(EFileSystemParameter.AssetbundleDecryptor, new TestFileStreamDecryption());
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
    private class TestRemoteService : IRemoteService
    {
        private readonly string _localServerRoot;

        public TestRemoteService(string localServerRoot)
        {
            _localServerRoot = localServerRoot;
        }

        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            List<string> urls = new List<string>();
            urls.Add($"{_localServerRoot}/{fileName}");
            return urls;
        }
    }

    [UnityTest]
    public IEnumerator C01_TestBundlePlaying()
    {
        var tester = new TestBundlePlaying();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C02_TestResourceImporter()
    {
        var tester = new TestResourceImporter();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C03_TestResourceDownloader()
    {
        var tester = new TestResourceDownloader();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C04_TestClearCache()
    {
        var tester = new TestClearCache();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C05_TestClearManifest()
    {
        var tester = new TestClearManifest();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator Z_DestroyPackage()
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