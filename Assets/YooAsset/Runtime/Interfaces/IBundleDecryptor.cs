using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 资源包解密操作的输入参数
    /// </summary>
    public readonly struct BundleDecryptArgs
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        internal PackageBundle Bundle { get; }

        /// <summary>
        /// 资源包的二进制数据
        /// </summary>
        /// <remarks>
        /// 仅在内存解密模式下有效
        /// </remarks>
        public byte[] FileData { get; }

        /// <summary>
        /// 资源包的文件路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 创建 <see cref="BundleDecryptArgs"/> 实例
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <param name="fileData">资源包的二进制数据</param>
        /// <param name="filePath">资源包的文件路径</param>
        internal BundleDecryptArgs(PackageBundle bundle, byte[] fileData, string filePath)
        {
            Bundle = bundle;
            FileData = fileData;
            FilePath = filePath;
        }
    }

    /// <summary>
    /// 资源包解密器的基接口，本身不包含成员。
    /// </summary>
    /// <remarks>
    /// 请实现以下派生接口之一：
    /// <see cref="IBundleOffsetDecryptor"/>、
    /// <see cref="IBundleMemoryDecryptor"/>、
    /// <see cref="IBundleStreamDecryptor"/>。
    /// </remarks>
    public interface IBundleDecryptor
    {
    }

    /// <summary>
    /// 基于偏移量的资源包解密器。
    /// 用于跳过文件头部加密区域后直接加载 AssetBundle。
    /// </summary>
    public interface IBundleOffsetDecryptor : IBundleDecryptor
    {
        /// <summary>
        /// 获取解密数据的起始偏移量（字节）
        /// </summary>
        /// <param name="args">解密操作的输入参数</param>
        /// <returns>AssetBundle 有效数据在文件中的起始偏移量</returns>
        long GetFileOffset(BundleDecryptArgs args);
    }

    /// <summary>
    /// 基于内存的资源包解密器。
    /// 将整个加密数据解密为字节数组后加载。
    /// </summary>
    public interface IBundleMemoryDecryptor : IBundleDecryptor
    {
        /// <summary>
        /// 将资源包数据解密并返回解密后的字节数组
        /// </summary>
        /// <param name="args">解密操作的输入参数</param>
        /// <returns>解密后的资源包数据</returns>
        byte[] GetDecryptedData(BundleDecryptArgs args);
    }

    /// <summary>
    /// 基于流的资源包解密器。
    /// 通过提供解密流实现流式加载，适用于大文件场景。
    /// </summary>
    public interface IBundleStreamDecryptor : IBundleDecryptor
    {
        /// <summary>
        /// 获取流式读取时使用的缓冲区大小（字节）
        /// </summary>
        /// <param name="args">解密操作的输入参数</param>
        /// <returns>建议的缓冲区字节数</returns>
        int GetBufferSize(BundleDecryptArgs args);

        /// <summary>
        /// 创建用于解密读取的流实例
        /// </summary>
        /// <remarks>
        /// 调用方拥有返回流的所有权，负责在使用完毕后释放资源。
        /// </remarks>
        /// <param name="args">解密操作的输入参数</param>
        /// <returns>可供 AssetBundle 加载使用的解密流</returns>
        Stream CreateDecryptionStream(BundleDecryptArgs args);
    }
}
