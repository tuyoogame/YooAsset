using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源包内全资源加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAllAssetsAsync / LoadAllAssetsSync
/// 测试内容:
/// 1. 异步加载资源包内所有 GameObject，验证加载状态和资源数量为 3
/// 2. 同步加载资源包内所有 GameObject，验证加载状态和资源数量为 3
/// </remarks>
public class TestLoadAllAssets
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载所有资源
        {
            var allAssetsHandle = package.LoadAllAssetsAsync<GameObject>("prefab_a");
            yield return allAssetsHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, allAssetsHandle.Status);

            var allAssetObjects = allAssetsHandle.AllAssetObjects;
            Assert.IsNotNull(allAssetObjects);

            int count = allAssetObjects.Count;
            Assert.AreEqual(3, count);
            allAssetsHandle.Release();
        }

        // 同步加载所有资源
        {
            var allAssetsHandle = package.LoadAllAssetsSync<GameObject>("prefab_x");
            Assert.AreEqual(EOperationStatus.Succeeded, allAssetsHandle.Status);

            var allAssetObjects = allAssetsHandle.AllAssetObjects;
            Assert.IsNotNull(allAssetObjects);

            int count = allAssetObjects.Count;
            Assert.AreEqual(3, count);
            allAssetsHandle.Release();
        }
    }
}