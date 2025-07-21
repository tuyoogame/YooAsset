using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试本地文件导入
/// </summary>
public class TestBundleImporter
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
        Assert.IsNotNull(package);

        string packageRoot = UnityEditor.EditorPrefs.GetString(T2_TestBuldinFileSystem.ASSET_BUNDLE_PACKAGE_ROOT_KEY);
        DirectoryInfo packageDir = new DirectoryInfo(packageRoot);
        string fileRoot = $"{packageDir.Parent.FullName}/OutputCache";

        ImportFileInfo fileInfoA = new ImportFileInfo();
        fileInfoA.FilePath = $"{fileRoot}/assets_samples_test_sample_testres3_import_prefab_importa.bundle.encrypt";
        fileInfoA.BundleName = "assets_samples_test_sample_testres3_import_prefab_importa.bundle";

        ImportFileInfo fileInfoB = new ImportFileInfo();
        fileInfoB.FilePath = $"{fileRoot}/assets_samples_test_sample_testres3_import_prefab_importb.bundle.encrypt";
        fileInfoB.BundleName = "assets_samples_test_sample_testres3_import_prefab_importb.bundle";

        ImportFileInfo[] importInfos = { fileInfoA, fileInfoB };
        var unpacker = package.CreateResourceImporter(importInfos, 10, 1);
        Assert.AreEqual(unpacker.TotalDownloadCount, 2);

        unpacker.BeginDownload();
        yield return unpacker;
        Assert.AreEqual(EOperationStatus.Succeed, unpacker.Status);
    }
}

/* 资源代码流程
 * 本地文件导入（导入器触发）
BundleInfo::CreateDownloader()
{
    options.ImportFilePath = _importFilePath;
    return _cacheFileSystem.DownloadFileAsync(Bundle, options);	
}
CacheFileSystem::DownloadFileAsync()
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