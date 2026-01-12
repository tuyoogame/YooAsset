using System;
using System.Text;
using System.Collections;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试远端文件下载
/// </summary>
/// <remarks>
/// 覆盖 API: CreateResourceDownloader / ResourceDownloader.StartDownload
/// 测试内容:
/// 1. 创建资源下载器，验证需要下载的资源数量非零
/// 2. 启动下载并等待完成，验证下载状态为成功
/// </remarks>
public class TestResourceDownloader
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var options = new ResourceDownloaderOptions(10, 1);
        var downloader = package.CreateResourceDownloader(options);
        Assert.AreNotEqual(0, downloader.TotalDownloadCount);

        downloader.StartDownload();
        yield return downloader;
        Assert.AreEqual(EOperationStatus.Succeeded, downloader.Status);
    }
}