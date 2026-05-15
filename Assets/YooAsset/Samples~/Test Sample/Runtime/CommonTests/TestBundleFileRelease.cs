using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试 Bundle 文件句柄释放与重新加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadBundleFileAsync / BundleFileHandle.Release / UnloadUnusedAssetsAsync
/// 测试内容:
/// 1. 异步加载 Bundle 文件，验证加载成功
/// 2. 释放 BundleFileHandle 引用，等待一帧
/// 3. 调用 UnloadUnusedAssetsAsync 清理未使用资源
/// 4. 再次加载同一 Bundle 文件，验证加载成功
/// </remarks>
public class TestBundleFileRelease
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        var bundleFileHandle = package.LoadBundleFileAsync("raw_file_e");
        yield return bundleFileHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, bundleFileHandle.Status);

        // 释放
        bundleFileHandle.Release();
        yield return new WaitForEndOfFrame();

        var unloadOp = package.UnloadUnusedAssetsAsync();
        yield return unloadOp;
        Assert.AreEqual(EOperationStatus.Succeeded, unloadOp.Status);

        // 再次加载
        var reloadHandle = package.LoadBundleFileAsync("raw_file_e");
        yield return reloadHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, reloadHandle.Status);
        reloadHandle.Release();
    }
}
