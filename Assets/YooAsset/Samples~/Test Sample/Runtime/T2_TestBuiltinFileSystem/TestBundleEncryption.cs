using System;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试加载加密文件
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / LoadAssetSync / InstantiateSync
/// 测试内容:
/// 1. 异步加载加密的预制体（prefab_encryptA），验证加载成功并实例化
/// 2. 同步加载加密的预制体（prefab_encryptB），验证加载成功并实例化
/// </remarks>
public class TestBundleEncryption
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载加密的预制体
        // 说明：测试内置文件解压
        {
            var assetHandle = package.LoadAssetAsync<GameObject>("prefab_encryptA");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var options = new InstantiateOptions(true, Vector3.zero, Quaternion.identity);
            var go = assetHandle.InstantiateSync(options);
            Assert.IsNotNull(go);
            GameObject.Destroy(go);
            assetHandle.Release();
        }

        // 同步加载加密的预制体
        // 说明：测试内置文件解压
        {
            var assetHandle = package.LoadAssetSync<GameObject>("prefab_encryptB");
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var options = new InstantiateOptions(true, Vector3.zero, Quaternion.identity);
            var go = assetHandle.InstantiateSync(options);
            Assert.IsNotNull(go);
            GameObject.Destroy(go);
            assetHandle.Release();
        }
    }
}