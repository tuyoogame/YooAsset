using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试销毁资源包裹
/// </summary>
/// <remarks>
/// 覆盖 API: DestroyPackageAsync / YooAssets.RemovePackage
/// 测试内容:
/// 1. 销毁 AssetBundleTestPackage 包裹，验证销毁状态，然后移除包裹
/// 2. 根据参数决定是否销毁 RawBundleTestPackage 包裹
/// 3. 根据参数决定是否销毁 ArchiveBundleTestPackage 包裹
/// </remarks>
public class TestDestroyPackage
{
    public IEnumerator RuntimeTester(bool destroyRawPackage, bool destroyArchivePackage = false)
    {
        // 销毁旧资源包 ASSET_BUNDLE
        {
            var package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
            var destroyOp = package.DestroyPackageAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, destroyOp.Status);

            YooAssets.RemovePackage(TestConsts.AssetBundlePackageName);
        }

        // 销毁旧资源包 RAW_BUNDLE
        if (destroyRawPackage)
        {
            var package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
            var destroyOp = package.DestroyPackageAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, destroyOp.Status);

            YooAssets.RemovePackage(TestConsts.RawBundlePackageName);
        }

        // 销毁旧资源包 ARCHIVE_BUNDLE
        if (destroyArchivePackage)
        {
            var package = YooAssets.GetPackage(TestConsts.ArchiveBundlePackageName);
            var destroyOp = package.DestroyPackageAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeeded)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeeded, destroyOp.Status);

            YooAssets.RemovePackage(TestConsts.ArchiveBundlePackageName);
        }
    }
}