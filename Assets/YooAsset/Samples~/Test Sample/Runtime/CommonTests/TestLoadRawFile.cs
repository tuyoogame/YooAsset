using System;
using System.Text;
using System.IO;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

/// <summary>
/// 测试原生文件加载
/// </summary>
/// <remarks>
/// 覆盖 API: LoadRawFileAsync / LoadRawFileSync / LoadAssetAsync(RawFileObject) / LoadAssetSync(RawFileObject)
/// 测试内容:
/// 1. 异步加载原生文件，获取文件路径，验证文件存在且二进制数据非空（raw_file_a）
/// 2. 同步加载原生文件，获取文件路径，验证文件存在且二进制数据非空（raw_file_b）
/// 3. 异步通过 RawFileObject 加载，验证 GetBytes() 和 GetText() 均返回有效数据（raw_file_c）
/// 4. 同步通过 RawFileObject 加载，验证 GetBytes() 和 GetText() 均返回有效数据（raw_file_d）
/// </remarks>
public class TestLoadRawFile
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        // 测试异步加载
        {
            var rawFileHandle = package.LoadRawFileAsync("raw_file_a");
            yield return rawFileHandle;
            Assert.AreEqual(EOperationStatus.Succeeded, rawFileHandle.Status);

            var filePath = rawFileHandle.GetRawFilePath();
            Assert.IsNotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));

            byte[] fileBytes = File.ReadAllBytes(filePath);
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);
            rawFileHandle.Release();
        }

        // 测试同步加载
        {
            var rawFileHandle = package.LoadRawFileSync("raw_file_b");
            Assert.AreEqual(EOperationStatus.Succeeded, rawFileHandle.Status);

            var filePath = rawFileHandle.GetRawFilePath();
            Assert.IsNotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));

            byte[] fileBytes = File.ReadAllBytes(filePath);
            Assert.IsNotNull(fileBytes);
            Assert.Greater(fileBytes.Length, 0);
            rawFileHandle.Release();
        }

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
    }
}
