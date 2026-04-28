using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试子资源加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadSubAssetsAsync / LoadSubAssetsSync
/// 测试内容:
/// 1. 异步加载图片子对象（Sprite），验证加载状态和子对象数量为 3
/// 2. 同步加载图片子对象（Sprite），验证加载状态和子对象数量为 3
/// </remarks>
public class TestLoadSubAssets
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载子对象
        {
            var subAssetsHandle = package.LoadSubAssetsAsync<Sprite>("image_a");
            yield return subAssetsHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, subAssetsHandle.Status);

            var subAssetObjects = subAssetsHandle.SubAssetObjects;
            Assert.IsNotNull(subAssetObjects);

            int count = subAssetObjects.Count;
            Assert.AreEqual(3, count);
            subAssetsHandle.Release();
        }

        // 同步加载子对象
        {
            var subAssetsHandle = package.LoadSubAssetsSync<Sprite>("image_b");
            Assert.AreEqual(EOperationStatus.Succeeded, subAssetsHandle.Status);

            var subAssetObjects = subAssetsHandle.SubAssetObjects;
            Assert.IsNotNull(subAssetObjects);

            int count = subAssetObjects.Count;
            Assert.AreEqual(3, count);
            subAssetsHandle.Release();
        }
    }
}