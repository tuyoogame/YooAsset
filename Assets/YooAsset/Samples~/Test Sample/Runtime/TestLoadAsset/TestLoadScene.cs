using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

public class TestLoadScene
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载主场景
        {
            var sceneHandle = package.LoadSceneAsync("scene_a", LoadSceneMode.Single);
            yield return sceneHandle;
            Assert.AreEqual(EOperationStatus.Succeed, sceneHandle.Status);

            var scene = sceneHandle.SceneObject;
            Assert.IsNotNull(scene);
        }

        // 同步加载附加场景
        yield return new WaitForSeconds(0.2f);
        {
            var sceneHandle = package.LoadSceneSync("scene_b", LoadSceneMode.Additive);
            Assert.AreEqual(EOperationStatus.Succeed, sceneHandle.Status);

            var scene = sceneHandle.SceneObject;
            Assert.IsNotNull(scene);
        }

        // 异步加载附加场景
        yield return new WaitForSeconds(0.2f);
        SceneHandle cachedHandle;
        {
            cachedHandle = package.LoadSceneSync("scene_c", LoadSceneMode.Additive);
            yield return cachedHandle;
            Assert.AreEqual(EOperationStatus.Succeed, cachedHandle.Status);

            var scene = cachedHandle.SceneObject;
            Assert.IsNotNull(scene);
        }

        // 异步销毁附加场景
        yield return new WaitForSeconds(0.2f);
        {
            var unloadSceneOp = cachedHandle.UnloadAsync();
            yield return unloadSceneOp;
            Assert.AreEqual(EOperationStatus.Succeed, unloadSceneOp.Status);
        }
    }
}