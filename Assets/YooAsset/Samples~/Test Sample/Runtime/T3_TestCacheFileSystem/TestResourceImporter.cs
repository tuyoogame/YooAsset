using System;
using System.IO;
using System.Text;
using System.Collections;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试本地文件导入
/// </summary>
/// <remarks>
/// 覆盖 API: CreateResourceImporter / ResourceImporter.StartDownload
/// 测试内容:
/// 1. 构造 ImportBundleInfo 数组，创建资源导入器，验证需要导入的资源数量为 2
/// 2. 启动导入并等待完成，验证导入状态为成功
/// </remarks>
public class TestResourceImporter
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        string packageRoot = string.Empty;
#if UNITY_EDITOR
        packageRoot = UnityEditor.EditorPrefs.GetString(T2_TestBuiltinFileSystem.ASSET_BUNDLE_PACKAGE_ROOT_KEY);
#endif
        DirectoryInfo packageDir = new DirectoryInfo(packageRoot);
        string fileRoot = $"{packageDir.Parent.FullName}/OutputCache";

        var fileInfoA = new ImportBundleInfo(
            filePath: $"{fileRoot}/assets_samples_test_sample_testres3_importfiles_prefab_import_a.bundle.encrypt",
            bundleName: "assets_samples_test_sample_testres3_importfiles_prefab_import_a.bundle",
            bundleGuid: null);

        var fileInfoB = new ImportBundleInfo(
            filePath: $"{fileRoot}/assets_samples_test_sample_testres3_importfiles_prefab_import_b.bundle.encrypt",
            bundleName: "assets_samples_test_sample_testres3_importfiles_prefab_import_b.bundle",
            bundleGuid: null);

        ImportBundleInfo[] importInfos = { fileInfoA, fileInfoB };
        var options = new BundleImporterOptions(importInfos, 10, 1);
        var unpacker = package.CreateResourceImporter(options);
        Assert.AreEqual(2, unpacker.TotalDownloadCount);

        unpacker.StartDownload();
        yield return unpacker;
        Assert.AreEqual(EOperationStatus.Succeeded, unpacker.Status);
    }
}