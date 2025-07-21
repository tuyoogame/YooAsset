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

public class TestDestroyPackage
{
    public IEnumerator RuntimeTester(bool destroyRawPackage)
    {
        // 销毁旧资源包 ASSET_BUNDLE
        {
            var package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
            var destroyOp = package.DestroyAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeed)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, destroyOp.Status);

            bool result = YooAssets.RemovePackage(TestDefine.AssetBundlePackageName);
            Assert.IsTrue(result);
        }

        // 销毁旧资源包 RAW_BUNDLE
        if (destroyRawPackage)
        {
            var package = YooAssets.GetPackage(TestDefine.RawBundlePackageName);
            var destroyOp = package.DestroyAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeed)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, destroyOp.Status);

            bool result = YooAssets.RemovePackage(TestDefine.RawBundlePackageName);
            Assert.IsTrue(result);
        }
    }
}