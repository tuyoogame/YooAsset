using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试 Bundle 文件加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadBundleFileAsync / LoadBundleFileSync
/// 测试内容:
/// 1. 异步加载 Bundle 文件（raw_file_a）
/// 2. 同步加载 Bundle 文件（raw_file_b）
/// </remarks>
public class TestLoadBundleFile
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        // 测试异步加载
        {
            var bundleFileHandle = package.LoadBundleFileAsync("raw_file_a");
            yield return bundleFileHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, bundleFileHandle.Status);
            bundleFileHandle.Release();
        }

        // 测试同步加载
        {
            var bundleFileHandle = package.LoadBundleFileSync("raw_file_b");
            Assert.AreEqual(EOperationStatus.Succeeded, bundleFileHandle.Status);
            bundleFileHandle.Release();
        }
    }
}
