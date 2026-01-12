using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试重复加载同一资源
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync — 重复调用
/// 测试内容:
/// 1. 对同一地址连续两次异步加载，验证均加载成功
/// 2. 验证两次返回的 AssetObject 是同一个对象引用（Provider 共享）
/// </remarks>
public class TestDuplicateLoad
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var handle1 = package.LoadAssetAsync<AudioClip>("sound_c");
        yield return handle1;
        Assert.AreEqual(EOperationStatus.Succeeded, handle1.Status);

        var handle2 = package.LoadAssetAsync<AudioClip>("sound_c");
        yield return handle2;
        Assert.AreEqual(EOperationStatus.Succeeded, handle2.Status);

        // 验证返回同一资源对象
        Assert.AreEqual(handle1.AssetObject, handle2.AssetObject);
        handle1.Release();
        handle2.Release();
    }
}
