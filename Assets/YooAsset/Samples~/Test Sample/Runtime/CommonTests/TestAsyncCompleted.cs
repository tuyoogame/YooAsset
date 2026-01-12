using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试异步加载完成回调
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / AssetHandle.Completed 事件
/// 测试内容:
/// 1. 注册 Completed 回调，异步加载预制体资源
/// 2. 等待加载完成后，验证回调已被触发
/// 3. 在回调中验证加载状态和资源对象非空
/// </remarks>
public class TestAsyncCompleted
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        bool callbackFired = false;
        var assetHandle = package.LoadAssetAsync<GameObject>("canvas_b");
        assetHandle.Completed += (AssetHandle handle) =>
        {
            callbackFired = true;
            Assert.AreEqual(EOperationStatus.Succeeded, handle.Status);
            Assert.IsNotNull(handle.AssetObject);
        };
        yield return assetHandle;
        Assert.IsTrue(callbackFired);
        assetHandle.Release();
    }
}
