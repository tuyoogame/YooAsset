
using System;
using System.Text;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件条目信息（构建期临时数据结构）
    /// </summary>
    internal class ArchiveFileEntry
    {
        private byte[] _pathBytes;

        /// <summary>
        /// 文件路径
        /// </summary>
        public readonly string AssetPath;

        /// <summary>
        /// 文件数据长度
        /// </summary>
        public readonly long DataLength;

        /// <summary>
        /// 文件 CRC32
        /// </summary>
        public readonly uint FileCRC;

        /// <summary>
        /// 数据在归档文件中的绝对偏移
        /// </summary>
        public long DataOffset;

        /// <summary>
        /// 构造归档文件条目
        /// </summary>
        public ArchiveFileEntry(string assetPath, long dataLength, uint fileCRC)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("Asset path is null or empty.", nameof(assetPath));
            if (dataLength < 0)
                throw new ArgumentException($"Invalid data length {dataLength} for '{assetPath}'.", nameof(dataLength));
            if (dataLength > ArchiveBundleConsts.MaxChildFileSize)
                throw new ArgumentException($"Child file exceeds maximum size ({ArchiveBundleConsts.MaxChildFileSize} bytes): '{assetPath}' ({dataLength} bytes).", nameof(dataLength));

            AssetPath = assetPath;
            DataLength = dataLength;
            FileCRC = fileCRC;
        }

        /// <summary>
        /// 获取文件路径的 UTF8 字节缓存
        /// </summary>
        public byte[] GetPathBytes()
        {
            if (_pathBytes == null)
                _pathBytes = Encoding.UTF8.GetBytes(AssetPath);
            return _pathBytes;
        }
    }
}
