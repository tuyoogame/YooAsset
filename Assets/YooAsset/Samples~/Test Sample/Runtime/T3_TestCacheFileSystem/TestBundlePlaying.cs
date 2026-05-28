using System;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试边玩边下
/// </summary>
/// <remarks>
/// 覆盖 API: GetDownloadSize / LoadAssetAsync / LoadAssetSync / UnloadUnusedAssetsAsync
/// 测试内容:
/// 1. 验证目标远端资源的下载大小非零（尚未缓存）
/// 2. 异步加载远端资源（prefab_encrypt_x），验证首次加载触发下载并最终成功
/// 3. 同步加载远端资源（prefab_encrypt_y），首次应失败并触发后台下载
/// 4. 释放失败的 Handle 并清理资源，等待下载完成后再次同步加载，验证成功
/// </remarks>
public class TestBundlePlaying
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        if (package.GetDownloadSize("prefab_encrypt_x") == 0)
        {
            Assert.Fail("Load bundle is already existed !");
        }
        if (package.GetDownloadSize("prefab_encrypt_y") == 0)
        {
            Assert.Fail("Load bundle is already existed !");
        }

        // 测试异步加载远端资源
        {
            var assetsHandle = package.LoadAssetAsync<GameObject>("prefab_encrypt_x");
            yield return assetsHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetsHandle.Status);
            assetsHandle.Release();
        }

        // 测试同步加载远端资源
        // 备注：同步加载会触发后台下载，二次加载的时候，本地资源应该保证成功。
        {
            // 验证失败结果
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            var assetsHandle = package.LoadAssetSync<GameObject>("prefab_encrypt_y");
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(EOperationStatus.Failed, assetsHandle.Status);

            // 清理加载器
            assetsHandle.Release();
            var unloadAssetsOp = package.UnloadUnusedAssetsAsync();
            yield return unloadAssetsOp;

            // 验证成功结果
            yield return new WaitForSeconds(1f);
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            assetsHandle = package.LoadAssetSync<GameObject>("prefab_encrypt_y");
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(EOperationStatus.Succeeded, assetsHandle.Status);
            assetsHandle.Release();
        }
    }
}