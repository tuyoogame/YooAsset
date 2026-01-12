using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试主资源加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync / LoadAssetSync
/// 测试内容:
/// 1. 异步加载音效资源（AudioClip），验证加载状态和资源对象非空
/// 2. 同步加载音效资源（AudioClip），验证同帧完成回调、Provider 已完成、加载状态和资源对象非空
/// </remarks>
public class TestLoadAsset
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载音效
        {
            var assetHandle = package.LoadAssetAsync<AudioClip>("sound_a");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var audioClip = assetHandle.AssetObject as AudioClip;
            Assert.IsNotNull(audioClip);
            assetHandle.Release();
        }

        // 同步加载音效
        {
            int loadFrame = Time.frameCount;
            var assetHandle = package.LoadAssetSync<AudioClip>("sound_b");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                Assert.AreEqual(loadFrame, Time.frameCount);
            };
            Assert.AreEqual(true, assetHandle.Provider.IsCompleted);
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var audioClip = assetHandle.AssetObject as AudioClip;
            Assert.IsNotNull(audioClip);
            assetHandle.Release();
        }
    }
}