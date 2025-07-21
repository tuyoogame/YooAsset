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

public class TestLoadSubAssets
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载子对象
        {
            var subAssetsHandle = package.LoadSubAssetsAsync<Sprite>("image_a");
            yield return subAssetsHandle;
            Assert.AreEqual(EOperationStatus.Succeed, subAssetsHandle.Status);

            var subAssetObjects = subAssetsHandle.SubAssetObjects;
            Assert.IsNotNull(subAssetObjects);

            int count = subAssetObjects.Count;
            Assert.AreEqual(count, 3);
        }

        // 同步加载子对象
        {
            var subAssetsHandle = package.LoadSubAssetsSync<Sprite>("image_b");
            Assert.AreEqual(EOperationStatus.Succeed, subAssetsHandle.Status);

            var subAssetObjects = subAssetsHandle.SubAssetObjects;
            Assert.IsNotNull(subAssetObjects);

            int count = subAssetObjects.Count;
            Assert.AreEqual(count, 3);
        }
    }
}