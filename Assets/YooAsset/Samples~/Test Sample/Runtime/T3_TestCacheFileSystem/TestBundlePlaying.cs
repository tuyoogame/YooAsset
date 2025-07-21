using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试边玩边下
/// </summary>
public class TestBundlePlaying
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        if (package.IsNeedDownloadFromRemote("prefab_encryptA") == false)
        {
            Assert.Fail("Load bundle is already existed !");
        }
        if (package.IsNeedDownloadFromRemote("prefab_encryptB") == false)
        {
            Assert.Fail("Load bundle is already existed !");
        }
        
        // 测试异步加载远端资源
        {
            var assetsHandle = package.LoadAssetAsync<GameObject>("prefab_encryptA");
            yield return assetsHandle;
            Assert.AreEqual(EOperationStatus.Succeed, assetsHandle.Status);
        }

        // 测试同步加载远端资源
        {
            // 验证失败结果
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            var assetsHandle = package.LoadAssetSync<GameObject>("prefab_encryptB");
            Assert.AreEqual(EOperationStatus.Failed, assetsHandle.Status);
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;

            // 清理加载器
            assetsHandle.Release();
            package.UnloadUnusedAssetsAsync();

            // 验证成功结果
            // 说明：同步加载也会触发远端下载任务！
            yield return new WaitForSeconds(1f);
            assetsHandle = package.LoadAssetSync<GameObject>("prefab_encryptB");
            Assert.AreEqual(EOperationStatus.Succeed, assetsHandle.Status);
        }
    }
}

/* 资源代码流程
 * 远端文件下载（加载器触发）
CacheFileSystem::LoadBundleFile()
{
	_cacheFileSystem.LoadBundleFile(bundle);
}
DCFSLoadAssetBundleOperation::InternalUpdate()
{
    if (_steps == ESteps.DownloadFile)
    {
	    DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
        _cacheFileSystem.DownloadFileAsync(_bundle, options);
    }
}
CacheFileSystem::DownloadFileAsync()
{
    if (string.IsNullOrEmpty(options.ImportFilePath))
    {
        //RemoteServices返回CDN文件路径
        string mainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        string fallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        options.SetURL(mainURL, fallbackURL);
        var downloader = new DownloadPackageBundleOperation(bundle, options);
        return downloader;
    }
}
*/