using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试场景加载与卸载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadSceneAsync / LoadSceneSync / SceneHandle.UnloadSceneAsync
/// 测试内容:
/// 1. 异步加载主场景（Single 模式），验证加载状态和场景对象非空
/// 2. 同步加载附加场景（Additive 模式），验证加载状态和场景对象非空
/// 3. 同步加载附加场景并等待完成，验证加载状态
/// 4. 异步卸载附加场景，验证卸载状态
/// </remarks>
public class TestLoadScene
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载主场景
        {
            var sceneHandle = package.LoadSceneAsync("scene_a", LoadSceneMode.Single);
            yield return sceneHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, sceneHandle.Status);

            var scene = sceneHandle.SceneObject;
            Assert.IsNotNull(scene);
        }

        // 同步加载附加场景
        yield return new WaitForSeconds(0.2f);
        {
            var sceneHandle = package.LoadSceneSync("scene_b", LoadSceneMode.Additive);
            Assert.AreEqual(EOperationStatus.Succeeded, sceneHandle.Status);

            var scene = sceneHandle.SceneObject;
            Assert.IsNotNull(scene);
        }

        // 同步加载附加场景并等待
        yield return new WaitForSeconds(0.2f);
        YooAsset.SceneHandle cachedHandle;
        {
            cachedHandle = package.LoadSceneSync("scene_c", LoadSceneMode.Additive);
            yield return cachedHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, cachedHandle.Status);

            var scene = cachedHandle.SceneObject;
            Assert.IsNotNull(scene);
        }

        // 异步销毁附加场景
        yield return new WaitForSeconds(0.2f);
        {
            var unloadSceneOp = cachedHandle.UnloadSceneAsync();
            yield return unloadSceneOp;
            Assert.AreEqual(EOperationStatus.Succeeded, unloadSceneOp.Status);
        }
    }
}