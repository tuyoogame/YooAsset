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

public class TestGetAssetInfos
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 获取所有资源对象信息
        var allAssetInfos = package.GetAllAssetInfos();
        Assert.AreEqual(allAssetInfos.Length, 28);

        // 获取指定资源对象信息
        var assetInfos = package.GetAssetInfos("import");
        Assert.AreEqual(assetInfos.Length, 2);

        yield break;
    }
}