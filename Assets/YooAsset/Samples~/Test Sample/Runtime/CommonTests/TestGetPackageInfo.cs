using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试包裹信息查询
/// </summary>
/// <remarks>
/// 覆盖 API: PackageValid / GetPackageVersion / GetPackageNote / GetPackageDetails
/// 测试内容:
/// 1. 验证 PackageValid 属性返回 true
/// 2. 验证 GetPackageVersion 返回非空非空白字符串
/// 3. 验证 GetPackageNote 返回非空值
/// 4. 验证 GetPackageDetails 返回非空对象，并检查内部字段：
///    - PackageName 与包裹名称一致
///    - PackageVersion 与 GetPackageVersion 一致
///    - BuildPipeline 非空
///    - AssetTotalCount 大于 0
///    - BundleTotalCount 大于 0
/// </remarks>
public class TestGetPackageInfo
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 验证包裹有效性
        Assert.IsTrue(package.PackageValid);

        // 验证版本信息
        string version = package.GetPackageVersion();
        Assert.IsNotNull(version);
        Assert.IsNotEmpty(version);

        // 验证备注信息
        string note = package.GetPackageNote();
        Assert.IsNotNull(note);

        // 验证详细信息
        var details = package.GetPackageDetails();
        Assert.IsNotNull(details);
        Assert.AreEqual(TestConsts.AssetBundlePackageName, details.PackageName);
        Assert.AreEqual(version, details.PackageVersion);
        Assert.IsFalse(string.IsNullOrEmpty(details.BuildPipeline));
        Assert.Greater(details.AssetTotalCount, 0);
        Assert.Greater(details.BundleTotalCount, 0);

        yield break;
    }
}
