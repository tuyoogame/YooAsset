using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

public class TestAsyncTask
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // Task异步加载面板
        {
            var assetsHandle = package.LoadAssetAsync<GameObject>("canvas");
            var handleTask = assetsHandle.Task;
            while (!handleTask.IsCompleted)
                yield return null;
            yield return null;
            Assert.AreEqual(EOperationStatus.Succeed, assetsHandle.Status);

            var instantiateOp = assetsHandle.InstantiateAsync();
            var operationTask = instantiateOp.Task;
            while (!operationTask.IsCompleted)
                yield return null;
            yield return null;
            Assert.AreEqual(EOperationStatus.Succeed, instantiateOp.Status);

            Assert.IsNotNull(instantiateOp.Result);
            TestLogger.Log(this, instantiateOp.Result.name);
            GameObject.Destroy(instantiateOp.Result);
        }
    }
}