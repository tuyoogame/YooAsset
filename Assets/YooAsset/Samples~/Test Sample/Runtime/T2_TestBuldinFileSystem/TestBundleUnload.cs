using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;
using System.Linq;

public class TestBundleUnload
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        string targetBundleName = "assets_samples_test_sample_testres4_enemy.bundle";

        // 加载Enemy
        AssetHandle assetHandle;
        {
            assetHandle = package.LoadAssetAsync<GameObject>("enemy");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeed, assetHandle.Status);

            DebugPackageData debugData = package.GetDebugPackageData();
            var findItem = debugData.BundleInfos.Where(x => x.BundleName == targetBundleName);
            Assert.AreEqual(findItem.Count(), 1);
        }

        // 卸载Enemy
        {
            assetHandle.Release();
            package.TryUnloadUnusedAsset("enemy");

            DebugPackageData debugData = package.GetDebugPackageData();
            var findItem = debugData.BundleInfos.Where(x => x.BundleName == targetBundleName);
            Assert.AreEqual(findItem.Count(), 0);
        }
    }
}