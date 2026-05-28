using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试精灵图集加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync (SpriteAtlas)
/// 测试内容:
/// 1. 异步加载 SpriteAtlas 资源，验证加载状态和资源对象非空
/// 2. 从图集中获取三个精灵（sprite_a、sprite_b、sprite_c），验证均非空
/// </remarks>
public class TestLoadSpriteAtlas
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var assetHandle = package.LoadAssetAsync<SpriteAtlas>("atlas_icon");
        yield return assetHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

        var spriteAtlas = assetHandle.AssetObject as SpriteAtlas;
        Assert.IsNotNull(spriteAtlas);

        var sprite1 = spriteAtlas.GetSprite("sprite_a");
        Assert.IsNotNull(sprite1);

        var sprite2 = spriteAtlas.GetSprite("sprite_b");
        Assert.IsNotNull(sprite2);

        var sprite3 = spriteAtlas.GetSprite("sprite_c");
        Assert.IsNotNull(sprite3);
        assetHandle.Release();
    }
}