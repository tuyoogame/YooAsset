using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源句柄释放与未使用资源卸载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / AssetHandle.Release / UnloadUnusedAssetsAsync
/// 测试内容:
/// 1. 异步加载音效资源，验证加载成功
/// 2. 释放 Handle 引用，等待一帧
/// 3. 调用 UnloadUnusedAssetsAsync 清理引用计数为零的资源
/// 4. 再次加载同一资源，验证重新加载成功且资源对象有效
/// </remarks>
public class TestHandleRelease
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 加载资源
        var assetHandle = package.LoadAssetAsync<AudioClip>("sound_e");
        yield return assetHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

        // 释放 Handle
        assetHandle.Release();
        yield return new WaitForEndOfFrame();

        // 清理未使用资源
        var unloadOp = package.UnloadUnusedAssetsAsync();
        yield return unloadOp;
        Assert.AreEqual(EOperationStatus.Succeeded, unloadOp.Status);

        // 再次加载验证可用性
        var reloadHandle = package.LoadAssetAsync<AudioClip>("sound_e");
        yield return reloadHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, reloadHandle.Status);
        Assert.IsNotNull(reloadHandle.AssetObject);
        reloadHandle.Release();
    }
}
