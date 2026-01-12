using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试原生文件句柄释放与重新加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadRawFileAsync / RawFileHandle.Release / UnloadUnusedAssetsAsync
/// 测试内容:
/// 1. 异步加载原生文件，验证加载成功
/// 2. 释放 RawFileHandle 引用，等待一帧
/// 3. 调用 UnloadUnusedAssetsAsync 清理未使用资源
/// 4. 再次加载同一原生文件，验证加载成功且文件路径有效
/// </remarks>
public class TestRawFileRelease
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        var rawFileHandle = package.LoadRawFileAsync("raw_file_e");
        yield return rawFileHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, rawFileHandle.Status);

        // 释放
        rawFileHandle.Release();
        yield return new WaitForEndOfFrame();

        var unloadOp = package.UnloadUnusedAssetsAsync();
        yield return unloadOp;
        Assert.AreEqual(EOperationStatus.Succeeded, unloadOp.Status);

        // 再次加载
        var reloadHandle = package.LoadRawFileAsync("raw_file_e");
        yield return reloadHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, reloadHandle.Status);
        Assert.IsTrue(File.Exists(reloadHandle.GetRawFilePath()));
        reloadHandle.Release();
    }
}
