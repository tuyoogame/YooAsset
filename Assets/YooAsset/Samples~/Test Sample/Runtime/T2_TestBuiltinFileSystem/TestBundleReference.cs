using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试资源引用关系与卸载后重新加载的完整性
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / InstantiateSync / AssetHandle.Release / UnloadUnusedAssetsAsync
/// 测试内容:
/// 1. 加载 HeroA 并实例化，验证成功
/// 2. 加载 HeroB 并实例化，验证成功
/// 3. 释放 HeroB 的 Handle 并销毁实例，调用 UnloadUnusedAssetsAsync 卸载
/// 4. 重新加载 HeroB 并实例化，验证材质球关联的纹理未丢失（依赖完整性）
/// </remarks>
public class TestBundleReference
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 加载HeroA
        AssetHandle heroAHandle;
        GameObject heroAObject;
        {
            heroAHandle = package.LoadAssetAsync<GameObject>("hero_a");
            yield return heroAHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, heroAHandle.Status);

            var pos = new Vector3(-1, -1, 0);
            var options = new InstantiateOptions(true, pos, Quaternion.identity);
            heroAObject = heroAHandle.InstantiateSync(options);
            Assert.IsNotNull(heroAObject);
        }

        // 加载HeroB
        AssetHandle heroHandle;
        GameObject heroObject;
        {
            heroHandle = package.LoadAssetAsync<GameObject>("hero_b");
            yield return heroHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, heroHandle.Status);

            var pos = new Vector3(1, -1, 0);
            var options = new InstantiateOptions(true, pos, Quaternion.identity);
            heroObject = heroHandle.InstantiateSync(options);
            Assert.IsNotNull(heroObject);
        }

        // 卸载HeroB
        {
            heroHandle.Release();
            GameObject.Destroy(heroObject);
            yield return new WaitForEndOfFrame();
        }

        // 清理未使用资源
        {
            var operation = package.UnloadUnusedAssetsAsync();
            yield return operation;
            Assert.AreEqual(EOperationStatus.Succeeded, operation.Status);
        }

        // 再次加载HeroB
        {
            heroHandle = package.LoadAssetAsync<GameObject>("hero_b");
            yield return heroHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, heroHandle.Status);

            var pos = new Vector3(1, -1, 0);
            var options = new InstantiateOptions(true, pos, Quaternion.identity);
            heroObject = heroHandle.InstantiateSync(options);
            Assert.IsNotNull(heroObject);

            // 检测材质球关联的纹理是否为空
            var mat = heroObject.GetComponent<MeshRenderer>().material;
            Assert.IsNotNull(mat.mainTexture);

            GameObject.Destroy(heroObject);
            heroHandle.Release();
        }

        // 清理HeroA
        {
            GameObject.Destroy(heroAObject);
            heroAHandle.Release();
        }
    }
}