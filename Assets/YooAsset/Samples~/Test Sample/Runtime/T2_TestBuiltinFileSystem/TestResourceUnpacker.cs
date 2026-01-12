using System;
using System.Text;
using System.Collections;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试内置文件解压
/// </summary>
/// <remarks>
/// 覆盖 API: CreateResourceUnpacker / ResourceUnpacker.StartDownload
/// 测试内容:
/// 1. 在 Android/OpenHarmony 平台下，验证需要解压的资源数量为 2，执行解压并验证成功
/// 2. 在非 Android/OpenHarmony 平台下，验证需要解压的资源数量为 0
/// </remarks>
public class TestResourceUnpacker
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var options = new ResourceUnpackerOptions("unpack", 10, 1);
        var resourceUnpacker = package.CreateResourceUnpacker(options);

#if UNITY_ANDROID || UNITY_OPENHARMONY
        Assert.AreEqual(2, resourceUnpacker.TotalDownloadCount);
        resourceUnpacker.StartDownload();
        yield return resourceUnpacker;
        Assert.AreEqual(EOperationStatus.Succeeded, resourceUnpacker.Status);
#else
        Assert.AreEqual(0, resourceUnpacker.TotalDownloadCount);
        yield break;
#endif
    }
}