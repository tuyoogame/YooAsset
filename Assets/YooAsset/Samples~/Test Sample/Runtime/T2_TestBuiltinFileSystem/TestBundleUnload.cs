using System;
using System.Text;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试单个资源精确卸载与诊断数据验证
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / AssetHandle.Release / TryUnloadUnusedAsset / GetDiagnosticData
/// 测试内容:
/// 1. 加载 Enemy 资源，通过诊断数据验证目标 Bundle 存在
/// 2. 释放 Handle 后调用 TryUnloadUnusedAsset 精确卸载
/// 3. 再次检查诊断数据，验证目标 Bundle 已被移除
/// </remarks>
public class TestBundleUnload
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        string targetBundleName = "assets_samples_test_sample_testres4_enemy.bundle";

        // 加载Enemy
        AssetHandle assetHandle;
        {
            assetHandle = package.LoadAssetAsync<GameObject>("enemy");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            DiagnosticPackageData debugData = package.GetDiagnosticData();
            var findItem = debugData.BundleInfos.Where(x => x.BundleName == targetBundleName);
            Assert.AreEqual(1, findItem.Count());
        }

        // 卸载Enemy
        {
            assetHandle.Release();
            package.TryUnloadUnusedAsset("enemy");

            DiagnosticPackageData debugData = package.GetDiagnosticData();
            var findItem = debugData.BundleInfos.Where(x => x.BundleName == targetBundleName);
            Assert.AreEqual(0, findItem.Count());
        }
    }
}