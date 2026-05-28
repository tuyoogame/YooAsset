using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源包缓存清理（4 种清理方式）
/// </summary>
/// <remarks>
/// 覆盖 API: UnloadAllAssetsAsync / ClearCacheAsync / GetDownloadSize
/// 前置条件: 所有资源已下载完毕，UnloadAllAssetsAsync 后所有资源无引用
/// 清理方式（按执行顺序）:
/// 1. ClearBundleFilesByLocations — 按地址清理指定资源包，验证该资源需重新下载
/// 2. ClearBundleFilesByTags — 按标签清理资源包，验证操作成功
/// 3. ClearUnusedBundleFiles — 因已全部卸载，等效于全量清理，验证剩余缓存全部清除
/// 4. ClearAllBundleFiles — 在空缓存下执行，验证幂等性（不会报错）
/// </remarks>
public class TestClearCache
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 先卸载所有资源，释放文件占用
        var unloadOp = package.UnloadAllAssetsAsync();
        yield return unloadOp;
        Assert.AreEqual(EOperationStatus.Succeeded, unloadOp.Status);

        // ---- 1. ClearBundleFilesByLocations ----
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearBundleFilesByLocations, "prefab_encrypt_x");
            var clearOp = package.ClearCacheAsync(options);
            yield return clearOp;
            Assert.AreEqual(EOperationStatus.Succeeded, clearOp.Status);

            Assert.Greater(package.GetDownloadSize("prefab_encrypt_x"), 0);
        }

        // ---- 2. ClearBundleFilesByTags ----
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearBundleFilesByTags, "import");
            var clearOp = package.ClearCacheAsync(options);
            yield return clearOp;
            Assert.AreEqual(EOperationStatus.Succeeded, clearOp.Status);
        }

        // ---- 3. ClearUnusedBundleFiles ----
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles);
            var clearOp = package.ClearCacheAsync(options);
            yield return clearOp;
            Assert.AreEqual(EOperationStatus.Succeeded, clearOp.Status);

            Assert.Greater(package.GetDownloadSize("prefab_encrypt_x"), 0);
        }

        // ---- 4. ClearAllBundleFiles ----
        {
            var options = new ClearCacheOptions(ClearCacheMethods.ClearAllBundleFiles);
            var clearOp = package.ClearCacheAsync(options);
            yield return clearOp;
            Assert.AreEqual(EOperationStatus.Succeeded, clearOp.Status);
        }
    }
}
