using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源清单文件清理（2 种清理方式）
/// </summary>
/// <remarks>
/// 覆盖 API: ClearCacheAsync (ClearUnusedManifestFiles / ClearAllManifestFiles)
/// 清理方式:
/// 1. ClearUnusedManifestFiles — 清理未在使用的清单文件，验证操作成功
/// 2. ClearAllManifestFiles — 清理所有清单文件，验证操作成功
/// </remarks>
public class TestClearManifest
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // ---- 1. ClearUnusedManifestFiles ----
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedManifestFiles);
            var clearOp = package.ClearCacheAsync(options);
            yield return clearOp;
            Assert.AreEqual(EOperationStatus.Succeeded, clearOp.Status);
        }

        // ---- 2. ClearAllManifestFiles ----
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearAllManifestFiles);
            var clearOp = package.ClearCacheAsync(options);
            yield return clearOp;
            Assert.AreEqual(EOperationStatus.Succeeded, clearOp.Status);
        }
    }
}
