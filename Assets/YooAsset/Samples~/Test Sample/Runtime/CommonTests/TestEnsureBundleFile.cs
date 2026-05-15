using System.IO;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试确保资源包已就绪
/// </summary>
/// <remarks>
/// 覆盖 API: EnsureBundleFileAsync
/// 测试内容:
/// 1. RawBundle 包裹：异步确保资源包已就绪，验证文件路径有效（raw_file_a）
/// 2. AssetBundle 包裹：异步确保资源包已就绪，验证文件路径有效（prefab_a）
/// 3. ArchiveBundle 包裹：异步确保资源包已就绪，验证文件路径有效（archive_file_a）
/// </remarks>
public class TestEnsureBundleFile
{
    public IEnumerator RuntimeTester_RawBundle()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        {
            var ensureOp = package.EnsureBundleFileAsync(new EnsureBundleFileOptions("raw_file_a"));
            yield return ensureOp;
            Assert.AreEqual(EOperationStatus.Succeeded, ensureOp.Status);

            var detail = ensureOp.Detail;
            Assert.IsNotNull(detail.BundleFilePath);
            Assert.IsNotEmpty(detail.BundleFilePath);
            Assert.IsTrue(File.Exists(detail.BundleFilePath), $"Bundle file does not exist: {detail.BundleFilePath}");
            Assert.IsNotNull(detail.BundleName);
            Assert.IsNotEmpty(detail.BundleName);
        }
    }

    public IEnumerator RuntimeTester_AssetBundle()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.AssetBundlePackageName);
        Assert.IsNotNull(package);

        {
            var ensureOp = package.EnsureBundleFileAsync(new EnsureBundleFileOptions("prefab_a"));
            yield return ensureOp;
            Assert.AreEqual(EOperationStatus.Succeeded, ensureOp.Status);

            var detail = ensureOp.Detail;
            Assert.IsNotNull(detail.BundleFilePath);
            Assert.IsNotEmpty(detail.BundleFilePath);
            Assert.IsTrue(File.Exists(detail.BundleFilePath), $"Bundle file does not exist: {detail.BundleFilePath}");
            Assert.IsNotNull(detail.BundleName);
            Assert.IsNotEmpty(detail.BundleName);
        }
    }

    public IEnumerator RuntimeTester_ArchiveBundle()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.ArchiveBundlePackageName);
        Assert.IsNotNull(package);

        {
            var ensureOp = package.EnsureBundleFileAsync(new EnsureBundleFileOptions("archive_file_a"));
            yield return ensureOp;
            Assert.AreEqual(EOperationStatus.Succeeded, ensureOp.Status);

            var detail = ensureOp.Detail;
            Assert.IsNotNull(detail.BundleFilePath);
            Assert.IsNotEmpty(detail.BundleFilePath);
            Assert.IsTrue(File.Exists(detail.BundleFilePath), $"Bundle file does not exist: {detail.BundleFilePath}");
            Assert.IsNotNull(detail.BundleName);
            Assert.IsNotEmpty(detail.BundleName);
        }
    }
}
