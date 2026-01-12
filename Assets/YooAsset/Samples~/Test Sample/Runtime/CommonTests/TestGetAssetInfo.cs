using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源信息查询（单资源、批量、标签、GUID）
/// </summary>
/// <remarks>
/// 覆盖 API: GetAssetInfo(location) / GetAssetInfo(location, type) / GetAssetInfoByGuid / GetAssetInfos(string) / GetAssetInfos(string[]) / GetAllAssetInfos
/// 测试内容:
/// 1. 查询无效地址，验证 IsValid 为 false
/// 2. 通过地址查询资源信息，验证 IsValid 和 AssetPath 非空
/// 3. 通过地址和类型查询资源信息，验证 IsValid 和 AssetPath 非空
/// 4. 通过 GUID 查询，根据 IncludeAssetGuid 开关分支验证：
///    - 开启时：有效 GUID 返回有效资源信息且 AssetPath 非空，无效 GUID 返回无效
///    - 关闭时：任何 GUID 查询均返回无效
/// 5. 单标签查询（"import"），验证返回 2 个资源且 PackageAsset 不是同一对象
/// 6. 多标签数组查询（string[] 重载），验证结果数量一致且 PackageAsset 不是同一对象
/// 7. 获取所有资源信息，验证总数与 TestConsts.TotalAssetCount 一致
/// </remarks>
public class TestGetAssetInfo
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 无效地址
        {
            var info = package.GetAssetInfo("__not_exist__");
            Assert.IsFalse(info.IsValid);
        }

        // GetAssetInfo(location)
        {
            var info = package.GetAssetInfo("sound");
            Assert.IsTrue(info.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(info.AssetPath));
        }

        // GetAssetInfo(location, type)
        {
            var info = package.GetAssetInfo("sound", typeof(AudioClip));
            Assert.IsTrue(info.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(info.AssetPath));
        }

        // GetAssetInfoByGuid
        {
            var details = package.GetPackageDetails();
            if (details.IncludeAssetGuid)
            {
                var guidInfo = package.GetAssetInfoByGuid(TestConsts.SoundAssetGuid);
                Assert.IsTrue(guidInfo.IsValid);
                Assert.IsFalse(string.IsNullOrEmpty(guidInfo.AssetPath));

                var invalidGuidInfo = package.GetAssetInfoByGuid("test_invalid_guid");
                Assert.IsFalse(invalidGuidInfo.IsValid);
            }
            else
            {
                var guidInfo = package.GetAssetInfoByGuid("test_invalid_guid");
                Assert.IsFalse(guidInfo.IsValid);
            }
        }

        // GetAssetInfos(string) — 单标签查询
        {
            var singleTagInfos = package.GetAssetInfos("import");
            Assert.AreEqual(2, singleTagInfos.Length);
            Assert.AreNotSame(singleTagInfos[0].Asset, singleTagInfos[1].Asset);
        }

        // GetAssetInfos(string[]) — 多标签查询
        {
            string[] tags = new string[] { "import" };
            var multiTagInfos = package.GetAssetInfos(tags);
            Assert.AreEqual(2, multiTagInfos.Length);
            Assert.AreNotSame(multiTagInfos[0].Asset, multiTagInfos[1].Asset);
        }

        // GetAllAssetInfos
        {
            var allAssetInfos = package.GetAllAssetInfos();
            Assert.AreEqual(TestConsts.TotalAssetCount, allAssetInfos.Length);
        }

        yield break;
    }
}
