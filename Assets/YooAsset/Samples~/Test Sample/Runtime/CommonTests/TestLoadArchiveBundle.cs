using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试归档资源包加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync(RawFileObject) / LoadAssetSync(RawFileObject) / UnloadUnusedAssetsAsync
/// 测试内容:
/// 1. 异步加载归档子文件，验证 GetBytes() 和 GetText() 均返回有效数据（archive_file_a）
/// 2. 同步加载归档子文件，验证 GetBytes() 和 GetText() 均返回有效数据（archive_file_b）
/// 3. 重复加载同一归档子文件，验证缓存命中不会失败（archive_file_c）
/// 4. 释放句柄并卸载后重新加载，验证卸载保护和重载链路正常（archive_file_e）
/// 5. 加载加密归档子文件，验证 Memory 解密路径（archive_file_x / archive_file_y）
/// </remarks>
public class TestLoadArchiveBundle
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.ArchiveBundlePackageName);
        Assert.IsNotNull(package);

        // 异步加载归档子文件
        {
            var assetHandle = package.LoadAssetAsync<RawFileObject>("archive_file_a");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var rawFileObject = assetHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(rawFileObject);

            byte[] fileBytes = rawFileObject.GetBytes();
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);

            string fileText = rawFileObject.GetText();
            Assert.IsNotNull(fileText);
            Assert.IsNotEmpty(fileText);
            assetHandle.Release();
        }

        // 同步加载归档子文件
        {
            var assetHandle = package.LoadAssetSync<RawFileObject>("archive_file_b");
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var rawFileObject = assetHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(rawFileObject);

            byte[] fileBytes = rawFileObject.GetBytes();
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);

            string fileText = rawFileObject.GetText();
            Assert.IsNotNull(fileText);
            Assert.IsNotEmpty(fileText);
            assetHandle.Release();
        }

        // 重复加载同一归档子文件，验证缓存命中
        {
            var handle1 = package.LoadAssetAsync<RawFileObject>("archive_file_c");
            yield return handle1;
            Assert.AreEqual(EOperationStatus.Succeeded, handle1.Status);

            var handle2 = package.LoadAssetAsync<RawFileObject>("archive_file_c");
            yield return handle2;
            Assert.AreEqual(EOperationStatus.Succeeded, handle2.Status);

            var obj1 = handle1.GetAssetObject<RawFileObject>();
            var obj2 = handle2.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj2);
            Assert.AreSame(obj1, obj2);
            handle1.Release();
            handle2.Release();
        }

        // 释放后卸载再重新加载，验证新旧对象不是同一实例
        {
            var assetHandle = package.LoadAssetAsync<RawFileObject>("archive_file_e");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var previousObj = assetHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(previousObj);
            assetHandle.Release();
            yield return new WaitForEndOfFrame();

            var unloadOp = package.UnloadUnusedAssetsAsync();
            yield return unloadOp;
            Assert.AreEqual(EOperationStatus.Succeeded, unloadOp.Status);

            var reloadHandle = package.LoadAssetAsync<RawFileObject>("archive_file_e");
            yield return reloadHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, reloadHandle.Status);

            var reloadedObj = reloadHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(reloadedObj);
            Assert.Greater(reloadedObj.GetBytes().Length, 0);
            Assert.AreNotSame(previousObj, reloadedObj);
            reloadHandle.Release();
        }

        // 异步加载加密归档子文件，验证 Memory 解密路径
        {
            var assetHandle = package.LoadAssetAsync<RawFileObject>("archive_file_x");
            yield return assetHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var rawFileObject = assetHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(rawFileObject);

            byte[] fileBytes = rawFileObject.GetBytes();
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);

            string fileText = rawFileObject.GetText();
            Assert.IsNotNull(fileText);
            Assert.IsNotEmpty(fileText);
            Assert.AreEqual("this is archive file x !", fileText);
            assetHandle.Release();
        }

        // 同步加载加密归档子文件，验证 Memory 解密路径
        {
            var assetHandle = package.LoadAssetSync<RawFileObject>("archive_file_y");
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var rawFileObject = assetHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(rawFileObject);

            byte[] fileBytes = rawFileObject.GetBytes();
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);

            string fileText = rawFileObject.GetText();
            Assert.IsNotNull(fileText);
            Assert.IsNotEmpty(fileText);
            Assert.AreEqual("this is archive file y !", fileText);
            assetHandle.Release();
        }
    }
}
