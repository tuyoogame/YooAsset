using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试内置文件解压
/// </summary>
public class TestBundleUnpacker
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        var resourceUnpacker = package.CreateResourceUnpacker("unpack", 10, 1);
        Assert.AreEqual(resourceUnpacker.TotalDownloadCount, 2);

        resourceUnpacker.BeginDownload();
        yield return resourceUnpacker;
        Assert.AreEqual(EOperationStatus.Succeed, resourceUnpacker.Status);
    }
}

/* 资源代码流程
 * 内置文件解压（解压器触发）
BundleInfo::CreateDownloader()
{
    return _buildFileSystem.DownloadFileAsync(Bundle, options);	
}
BuildinFileSystem::DownloadFileAsync()
{
    options.ImportFilePath = GetBuildinFileLoadPath(bundle);
	return _unpackFileSystem.DownloadFileAsync(bundle, options);
}
UnpackFileSystem::DownloadFileAsync()
{
    if (string.IsNullOrEmpty(options.ImportFilePath) == false)
    {
	    string mainURL = ConvertToWWWPath(options.ImportFilePath);
 	    options.SetURL(mainURL, mainURL);
 	    var downloader = new DownloadPackageBundleOperation(bundle, options);
        return downloader;
    }
}
*/