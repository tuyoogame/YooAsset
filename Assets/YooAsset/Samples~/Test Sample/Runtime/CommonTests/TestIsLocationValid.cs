using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源定位地址有效性查询
/// </summary>
/// <remarks>
/// 覆盖 API: IsLocationValid
/// 测试内容:
/// 1. 验证有效地址返回 true
/// 2. 验证无效地址返回 false
/// </remarks>
public class TestIsLocationValid
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 有效地址
        Assert.IsTrue(package.IsLocationValid("canvas"));

        // 无效地址
        Assert.IsFalse(package.IsLocationValid("__not_exist_location__"));

        yield break;
    }
}
