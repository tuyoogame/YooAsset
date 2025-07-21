using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试远端文件下载
/// </summary>
public class TestBundleDownloader
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var downloader = package.CreateResourceDownloader(10, 1);
        Assert.AreNotEqual(downloader.TotalDownloadCount, 0);

        downloader.BeginDownload();
        yield return downloader;
        Assert.AreEqual(EOperationStatus.Succeed, downloader.Status);
    }
}

/* 资源代码流程
 * 远端文件下载（下载器触发）
BundleInfo::CreateDownloader()
{
    return _buildFileSystem.DownloadFileAsync(Bundle, options);	
}
CacheFileSystem::DownloadFileAsync()
{
    if (string.IsNullOrEmpty(options.ImportFilePath))
    {
	    //RemoteServices返回CDN文件路径
	    string mainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        string fallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        options.SetURL(mainURL, fallbackURL);
        var downloader = new DownloadPackageBundleOperation(bundle, options);
        return downloader;
    }
}
*/