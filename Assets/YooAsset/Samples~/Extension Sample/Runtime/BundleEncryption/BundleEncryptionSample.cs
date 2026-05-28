using System;
using System.IO;
using YooAsset;

/// <summary>
/// Bundle 加密和解密示例
/// 注意：示例代码仅演示接口用法，实际项目请替换密钥和加密算法。
/// </summary>
public static class BundleEncryptionSample
{
    private const byte XOR_KEY = 64;
    private const int FILE_OFFSET = 32;

    /// <summary>
    /// 内存模式加密器：将加密后的完整文件数据返回给构建管线。
    /// </summary>
    public class MemoryEncryptor : IBundleEncryptor
    {
        public BundleEncryptResult Encrypt(BundleEncryptArgs args)
        {
            byte[] data = File.ReadAllBytes(args.FilePath);
            Xor(data);
            return new BundleEncryptResult(true, data);
        }
    }

    /// <summary>
    /// 内存模式解密器：运行时返回解密后的完整文件数据。
    /// </summary>
    public class MemoryDecryptor : IBundleMemoryDecryptor
    {
        byte[] IBundleMemoryDecryptor.GetDecryptedData(BundleDecryptArgs args)
        {
            byte[] data = args.FileData;
            if (data == null)
                data = FileUtility.ReadAllBytes(args.FilePath);

            Xor(data);
            return data;
        }
    }

    /// <summary>
    /// 偏移模式加密器：在文件头部插入固定长度的数据。
    /// </summary>
    public class OffsetEncryptor : IBundleEncryptor
    {
        public BundleEncryptResult Encrypt(BundleEncryptArgs args)
        {
            byte[] data = File.ReadAllBytes(args.FilePath);
            byte[] encryptedData = new byte[data.Length + FILE_OFFSET];
            Buffer.BlockCopy(data, 0, encryptedData, FILE_OFFSET, data.Length);
            return new BundleEncryptResult(true, encryptedData);
        }
    }

    /// <summary>
    /// 偏移模式解密器：运行时返回真实 Bundle 数据起始偏移。
    /// </summary>
    public class OffsetDecryptor : IBundleOffsetDecryptor
    {
        long IBundleOffsetDecryptor.GetFileOffset(BundleDecryptArgs args)
        {
            return FILE_OFFSET;
        }
    }

    /// <summary>
    /// 流模式加密器：示例中使用 XOR 加密整个文件。
    /// </summary>
    public class StreamEncryptor : IBundleEncryptor
    {
        public BundleEncryptResult Encrypt(BundleEncryptArgs args)
        {
            byte[] data = File.ReadAllBytes(args.FilePath);
            Xor(data);
            return new BundleEncryptResult(true, data);
        }
    }

    /// <summary>
    /// 流模式解密器：运行时返回一个读取时自动解密的文件流。
    /// </summary>
    public class StreamDecryptor : IBundleStreamDecryptor
    {
        Stream IBundleStreamDecryptor.CreateDecryptionStream(BundleDecryptArgs args)
        {
            return new XorFileStream(args.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        int IBundleStreamDecryptor.GetBufferSize(BundleDecryptArgs args)
        {
            return 1024;
        }
    }

    private class XorFileStream : FileStream
    {
        public XorFileStream(string path, FileMode mode, FileAccess access, FileShare share)
            : base(path, mode, access, share)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            int read = base.Read(array, offset, count);
            for (int i = offset; i < offset + read; i++)
            {
                array[i] ^= XOR_KEY;
            }
            return read;
        }
    }

    private static void Xor(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= XOR_KEY;
        }
    }
}
