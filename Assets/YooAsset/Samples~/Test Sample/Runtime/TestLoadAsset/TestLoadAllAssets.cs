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

public class TestLoadAllAssets
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载所有资源
        {
            var allAssetsHandle = package.LoadAllAssetsAsync<GameObject>("prefab_a");
            yield return allAssetsHandle;
            Assert.AreEqual(EOperationStatus.Succeed, allAssetsHandle.Status);

            var allAssetObjects = allAssetsHandle.AllAssetObjects;
            Assert.IsNotNull(allAssetObjects);

            int count = allAssetObjects.Count;
            Assert.AreEqual(count, 3);
        }

        // 同步加载所有资源
        {
            var allAssetsHandle = package.LoadAllAssetsSync<GameObject>("prefab_x");
            Assert.AreEqual(EOperationStatus.Succeed, allAssetsHandle.Status);

            var allAssetObjects = allAssetsHandle.AllAssetObjects;
            Assert.IsNotNull(allAssetObjects);

            int count = allAssetObjects.Count;
            Assert.AreEqual(count, 3);
        }
    }
}