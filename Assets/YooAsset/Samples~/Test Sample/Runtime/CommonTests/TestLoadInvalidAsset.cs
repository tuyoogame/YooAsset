using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试加载不存在的资源（错误路径验证）
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / LoadAssetSync — 无效地址
/// 测试内容:
/// 1. 异步加载不存在的资源地址，验证返回 Failed 状态（使用 LogAssert 忽略预期错误日志）
/// 2. 同步加载不存在的资源地址，验证返回 Failed 状态
/// </remarks>
public class TestLoadInvalidAsset
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载不存在的资源
        {
            LogAssert.ignoreFailingMessages = true;
            var assetHandle = package.LoadAssetAsync<GameObject>("__not_exist_location__");
            yield return assetHandle;
            LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(EOperationStatus.Failed, assetHandle.Status);
        }

        // 同步加载不存在的资源
        {
            LogAssert.ignoreFailingMessages = true;
            var assetHandle = package.LoadAssetSync<GameObject>("__not_exist_location__");
            LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(EOperationStatus.Failed, assetHandle.Status);
        }
    }
}
