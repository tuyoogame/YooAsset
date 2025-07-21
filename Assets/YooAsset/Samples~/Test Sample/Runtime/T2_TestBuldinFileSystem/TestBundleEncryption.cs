using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试加载加密文件
/// </summary>
public class TestBundleEncryption
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载加密的预制体
        // 说明：测试内置文件解压
        {
            var assetHandle = package.LoadAssetAsync<GameObject>("prefab_encryptA");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeed, assetHandle.Status);

            var go = assetHandle.InstantiateSync(Vector3.zero, Quaternion.identity);
            Assert.IsNotNull(go);
        }

        // 同步加载加密的预制体
        // 说明：测试内置文件解压
        {
            var assetHandle = package.LoadAssetSync<GameObject>("prefab_encryptB");
            Assert.AreEqual(EOperationStatus.Succeed, assetHandle.Status);

            var go = assetHandle.InstantiateSync(Vector3.zero, Quaternion.identity);
            Assert.IsNotNull(go);
        }
    }
}

/* 资源代码流程
 * 内置文件解压（加载器触发）
BuildinFileSystem::LoadBundleFile()
{
    if (IsUnpackBundleFile(bundle))
    {
	    _unpackFileSystem.LoadBundleFile(bundle);
    }
}
UnpackFileSystem::LoadBundleFile()
{
    var operation = new DCFSLoadAssetBundleOperation(this, bundle);
    return operation;
}
DCFSLoadAssetBundleOperation::InternalUpdate()
{
    if (_steps == ESteps.DownloadFile)
    {
	    DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
        _unpackFileSystem.DownloadFileAsync(_bundle, options);
    }
}
UnpackFileSystem::DownloadFileAsync()
{
    if (string.IsNullOrEmpty(options.ImportFilePath))
    {
        //RemoteServices返回内置文件路径
        string mainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        string fallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        options.SetURL(mainURL, fallbackURL);
        var downloader = new DownloadPackageBundleOperation(bundle, options);
        return downloader;
    }
}
*/