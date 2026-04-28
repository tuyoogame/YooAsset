using System;
using System.Text;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试 ScriptableObject 序列化对象加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync(location) / LoadAssetSync(location) — 无泛型无类型参数重载
/// 测试内容:
/// 1. 异步加载 ScriptableObject 配置文件，验证加载状态和类型转换正确
/// 2. 同步加载 ScriptableObject 配置文件，验证加载状态和类型转换正确
/// </remarks>
public class TestLoadScriptableObject
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载序列化对象
        {
            var assetHandle = package.LoadAssetAsync("config_a");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var testScriptableObject = assetHandle.AssetObject as TestScriptableObject;
            Assert.IsNotNull(testScriptableObject);
            TestLogger.Log(this, testScriptableObject.ConfigName);
            assetHandle.Release();
        }

        // 同步加载序列化对象
        {
            var assetHandle = package.LoadAssetSync("config_b");
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var testScriptableObject = assetHandle.AssetObject as TestScriptableObject;
            Assert.IsNotNull(testScriptableObject);
            TestLogger.Log(this, testScriptableObject.ConfigName);
            assetHandle.Release();
        }
    }
}