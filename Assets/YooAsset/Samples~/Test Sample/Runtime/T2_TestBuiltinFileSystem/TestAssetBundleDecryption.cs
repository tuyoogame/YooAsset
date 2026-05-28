using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试加载加密的 AssetBundle 资源
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / LoadAssetSync / InstantiateSync
/// 测试内容:
/// 1. 使用 Memory 解密器加载加密预制体（prefab_encrypt_x / prefab_encrypt_y）
/// </remarks>
public class TestAssetBundleDecryption
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        yield return TestEncryptedPrefabPair(package, "prefab_encrypt_x", "prefab_encrypt_y");
    }
    private IEnumerator TestEncryptedPrefabPair(ResourcePackage package, string asyncLocation, string syncLocation)
    {
        Assert.IsNotNull(package);

        // 异步加载加密的预制体
        {
            var assetHandle = package.LoadAssetAsync<GameObject>(asyncLocation);
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var options = new InstantiateOptions(true, Vector3.zero, Quaternion.identity);
            var go = assetHandle.InstantiateSync(options);
            Assert.IsNotNull(go);
            GameObject.Destroy(go);
            assetHandle.Release();
        }

        // 同步加载加密的预制体
        {
            var assetHandle = package.LoadAssetSync<GameObject>(syncLocation);
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var options = new InstantiateOptions(true, Vector3.zero, Quaternion.identity);
            var go = assetHandle.InstantiateSync(options);
            Assert.IsNotNull(go);
            GameObject.Destroy(go);
            assetHandle.Release();
        }
    }
}
