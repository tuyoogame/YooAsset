using System;
using System.IO;
using YooAsset;

/// <summary>
/// XOR 解密文件流，读取时自动对每个字节执行异或解密
/// </summary>
public class TestBundleStream : FileStream
{
    public const byte KEY = 64;

    public TestBundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
    {
    }
    public TestBundleStream(string path, FileMode mode) : base(path, mode)
    {
    }
    public override int Read(byte[] array, int offset, int count)
    {
        var index = base.Read(array, offset, count);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= KEY;
        }
        return index;
    }
}
/// <summary>
/// 流模式 XOR 加密器，对 TestRes3 目录的 Bundle 进行逐字节异或加密
/// </summary>
public class TestFileStreamEncryption : IBundleEncryptor
{
    public BundleEncryptResult Encrypt(BundleEncryptArgs fileInfo)
    {
        // 说明：对TestRes3资源目录进行加密
        if (fileInfo.BundleName.Contains("_testres3_"))
        {
            var fileData = File.ReadAllBytes(fileInfo.FilePath);
            for (int i = 0; i < fileData.Length; i++)
            {
                fileData[i] ^= TestBundleStream.KEY;
            }

            return new BundleEncryptResult(true, fileData);
        }
        else
        {
            return new BundleEncryptResult(false, null);
        }
    }
}
/// <summary>
/// 偏移模式加密器，在 Bundle 文件头部插入固定长度的空白偏移
/// </summary>
public class TestFileOffsetEncryption : IBundleEncryptor
{
    public BundleEncryptResult Encrypt(BundleEncryptArgs fileInfo)
    {
        // 说明：对TestRes3资源目录进行加密
        if (fileInfo.BundleName.Contains("_testres3_"))
        {
            int offset = 32;
            byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
            var encryptedData = new byte[fileData.Length + offset];
            Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

            return new BundleEncryptResult(true, encryptedData);
        }
        else
        {
            return new BundleEncryptResult(false, null);
        }
    }
}

/// <summary>
/// 偏移模式解密器，返回固定的文件偏移量以跳过加密头
/// </summary>
public class TestFileOffsetDecryption : IBundleOffsetDecryptor
{
    private const long FILE_OFFSET = 32;

    long IBundleOffsetDecryptor.GetFileOffset(BundleDecryptArgs args)
    {
        return FILE_OFFSET;
    }
}
/// <summary>
/// 内存模式 XOR 解密器，将整个 Bundle 读入内存后逐字节异或解密
/// </summary>
public class TestFileMemoryDecryption : IBundleMemoryDecryptor
{
    byte[] IBundleMemoryDecryptor.GetDecryptedData(BundleDecryptArgs args)
    {
        byte[] data = args.FileData;

        // 注意：如果数据为空，自行加载文件数据。
        if (data == null)
            data = FileUtility.ReadAllBytes(args.FilePath);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= TestBundleStream.KEY;
        }
        return data;
    }
}
/// <summary>
/// 流模式解密器，返回 TestBundleStream 实现读取时自动解密
/// </summary>
public class TestFileStreamDecryption : IBundleStreamDecryptor
{
    Stream IBundleStreamDecryptor.CreateDecryptionStream(BundleDecryptArgs args)
    {
        var fileStream = new TestBundleStream(args.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return fileStream;
    }

    int IBundleStreamDecryptor.GetBufferSize(BundleDecryptArgs args)
    {
        return 1024;
    }
}