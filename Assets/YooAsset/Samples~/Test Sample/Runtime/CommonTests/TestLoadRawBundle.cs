using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试 RawBundle 加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadAssetAsync(RawFileObject) / LoadAssetSync(RawFileObject)
/// 测试内容:
/// 1. 异步通过 RawFileObject 加载，验证 GetBytes() 和 GetText() 均返回有效数据（raw_file_c）
/// 2. 同步通过 RawFileObject 加载，验证 GetBytes() 和 GetText() 均返回有效数据（raw_file_d）
/// 3. 异步加载加密 RawBundle，验证 Memory 解密路径（raw_file_x）
/// 4. 同步加载加密 RawBundle，验证 Memory 解密路径（raw_file_y）
/// </remarks>
public class TestLoadRawBundle
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        // 测试异步加载：通过 RawFileObject 获取二进制数据和文本数据
        {
            var assetHandle = package.LoadAssetAsync<RawFileObject>("raw_file_c");
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

        // 测试同步加载：通过 RawFileObject 获取二进制数据和文本数据
        {
            var assetHandle = package.LoadAssetSync<RawFileObject>("raw_file_d");
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

        // 测试异步加载加密 RawBundle：通过 RawFileObject 获取二进制数据和文本数据
        {
            var assetHandle = package.LoadAssetAsync<RawFileObject>("raw_file_x");
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
            Assert.AreEqual("this is raw file x !", fileText);
            assetHandle.Release();
        }

        // 测试同步加载加密 RawBundle：通过 RawFileObject 获取二进制数据和文本数据
        {
            var assetHandle = package.LoadAssetSync<RawFileObject>("raw_file_y");
            Assert.AreEqual(EOperationStatus.Succeeded, assetHandle.Status);

            var rawFileObject = assetHandle.GetAssetObject<RawFileObject>();
            Assert.IsNotNull(rawFileObject);

            byte[] fileBytes = rawFileObject.GetBytes();
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);

            string fileText = rawFileObject.GetText();
            Assert.IsNotNull(fileText);
            Assert.IsNotEmpty(fileText);
            Assert.AreEqual("this is raw file y !", fileText);
            assetHandle.Release();
        }
    }
}
