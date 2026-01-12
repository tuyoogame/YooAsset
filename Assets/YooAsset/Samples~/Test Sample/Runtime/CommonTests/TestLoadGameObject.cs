using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试 yield return 模式异步实例化
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / AssetHandle.InstantiateAsync
/// 测试内容:
/// 1. 异步加载 GameObject 资源，验证加载成功
/// 2. 调用 InstantiateAsync 异步实例化，yield return 等待完成
/// 3. 验证实例化状态和结果对象非空，然后销毁实例
/// </remarks>
public class TestLoadGameObject
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var assetHandle = package.LoadAssetAsync<GameObject>("canvas");
        yield return assetHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

        var instantiateOp = assetHandle.InstantiateAsync();
        yield return instantiateOp;
        Assert.AreEqual(EOperationStatus.Succeeded, instantiateOp.Status);
        Assert.IsNotNull(instantiateOp.Result);

        GameObject.Destroy(instantiateOp.Result);
        assetHandle.Release();
    }
}
