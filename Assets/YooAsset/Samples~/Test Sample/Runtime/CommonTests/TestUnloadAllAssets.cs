using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试全量卸载资源
/// </summary>
/// <remarks>
/// 覆盖 API: UnloadAllAssetsAsync / LoadAssetAsync
/// 测试内容:
/// 1. 异步加载一个音乐资源，验证加载成功
/// 2. 调用 UnloadAllAssetsAsync 全量卸载所有资源，验证操作成功
/// 3. 重新加载同一资源，验证卸载后可正常重新加载
/// </remarks>
public class TestUnloadAllAssets
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 先加载一个资源
        var assetHandle = package.LoadAssetAsync<AudioClip>("sound_d");
        yield return assetHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);
        var firstAsset = assetHandle.AssetObject;
        Assert.IsNotNull(firstAsset);

        // 全量卸载
        var unloadAllOp = package.UnloadAllAssetsAsync();
        yield return unloadAllOp;
        Assert.AreEqual(EOperationStatus.Succeeded, unloadAllOp.Status);

        // 重新加载验证
        var reloadHandle = package.LoadAssetAsync<AudioClip>("sound_d");
        yield return reloadHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, reloadHandle.Status);
        Assert.IsNotNull(reloadHandle.AssetObject);
        reloadHandle.Release();
    }
}
