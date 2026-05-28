using System;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 归档文件资源包数据类
    /// </summary>
    internal class ArchiveBundle
    {
        /// <summary>
        /// 归档文件中单个子文件的索引条目
        /// </summary>
        internal readonly struct FileEntry
        {
            /// <summary>
            /// 子文件的资源路径
            /// </summary>
            public readonly string AssetPath;

            /// <summary>
            /// 子文件数据在归档文件中的起始偏移
            /// </summary>
            public readonly long DataOffset;

            /// <summary>
            /// 子文件数据长度（字节）
            /// </summary>
            public readonly long DataLength;

            /// <summary>
            /// 子文件数据的 CRC 校验值
            /// </summary>
            /// <remarks>
            /// 保留字段，运行时不参与读取校验。
            /// </remarks>
            public readonly uint FileCRC;

            public FileEntry(string assetPath, long dataOffset, long dataLength, uint fileCRC)
            {
                if (string.IsNullOrEmpty(assetPath))
                    throw new ArgumentException("Asset path is null or empty.", nameof(assetPath));
                if (dataLength < 0)
                    throw new ArgumentException($"Invalid data length {dataLength} for '{assetPath}'.", nameof(dataLength));
                if (dataLength > ArchiveBundleConsts.MaxChildFileSize)
                    throw new ArgumentException($"Child file exceeds maximum size ({ArchiveBundleConsts.MaxChildFileSize} bytes): '{assetPath}' ({dataLength} bytes).", nameof(dataLength));
                if (dataOffset < 0)
                    throw new ArgumentException($"Invalid data offset {dataOffset} for '{assetPath}'.", nameof(dataOffset));

                AssetPath = assetPath;
                DataOffset = dataOffset;
                DataLength = dataLength;
                FileCRC = fileCRC;
            }
        }

        private readonly string _archiveFilePath;
        private readonly byte[] _memoryData;
        private readonly Dictionary<string, FileEntry> _entries;
        private readonly Dictionary<string, RawFileObject> _cachedObjects = new Dictionary<string, RawFileObject>();
        private bool _isUnloaded;

        /// <summary>
        /// 从本地文件创建 ArchiveBundle 实例
        /// </summary>
        public ArchiveBundle(string archiveFilePath, Dictionary<string, FileEntry> entries)
        {
            if (string.IsNullOrEmpty(archiveFilePath))
                throw new ArgumentException("Archive file path is null or empty.", nameof(archiveFilePath));
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            _archiveFilePath = archiveFilePath;
            _memoryData = null;
            _entries = entries;
            _isUnloaded = false;
        }

        /// <summary>
        /// 从解密后的内存数据创建 ArchiveBundle 实例
        /// </summary>
        public ArchiveBundle(byte[] memoryData, Dictionary<string, FileEntry> entries)
        {
            if (memoryData == null)
                throw new ArgumentNullException(nameof(memoryData));
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            _archiveFilePath = null;
            _memoryData = memoryData;
            _entries = entries;
            _isUnloaded = false;
        }

        /// <summary>
        /// 根据资源路径创建原生文件对象，已创建的对象会被缓存
        /// </summary>
        /// <param name="assetPath">子文件的资源路径</param>
        /// <returns>创建得到的原生文件对象</returns>
        public RawFileObject CreateRawFileObject(string assetPath)
        {
            if (_isUnloaded)
                throw new InvalidOperationException($"{nameof(ArchiveBundle)} has been unloaded.");
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("Asset path is null or empty.", nameof(assetPath));

            if (_cachedObjects.TryGetValue(assetPath, out RawFileObject cached))
                return cached;

            byte[] assetData = ReadAssetData(assetPath);
            var rawFileObject = RawFileObject.CreateFromBytes(assetData);
            _cachedObjects[assetPath] = rawFileObject;
            return rawFileObject;
        }

        /// <summary>
        /// 卸载归档资源包
        /// </summary>
        public void Unload()
        {
            _isUnloaded = true;
            foreach (var obj in _cachedObjects.Values)
            {
                obj.Release();
                UnityEngine.Object.Destroy(obj);
            }
            _cachedObjects.Clear();
            _entries.Clear();
        }

        private byte[] ReadAssetData(string assetPath)
        {
            if (_entries.TryGetValue(assetPath, out FileEntry entry) == false)
                throw new InvalidOperationException($"Asset not found in archive: '{assetPath}'.");

            if (_memoryData != null)
                return ReadFromMemory(entry);
            else
                return ReadFromFile(entry);
        }
        private byte[] ReadFromMemory(FileEntry entry)
        {
            byte[] buffer = new byte[entry.DataLength];
            Buffer.BlockCopy(_memoryData, (int)entry.DataOffset, buffer, 0, (int)entry.DataLength);
            return buffer;
        }
        private byte[] ReadFromFile(FileEntry entry)
        {
            byte[] buffer = new byte[entry.DataLength];
            using (var fs = new FileStream(_archiveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Seek(entry.DataOffset, SeekOrigin.Begin);
                int bytesRead = 0;
                while (bytesRead < buffer.Length)
                {
                    int read = fs.Read(buffer, bytesRead, buffer.Length - bytesRead);
                    if (read == 0)
                        throw new EndOfStreamException($"Unexpected end of archive file while reading '{entry.AssetPath}'.");
                    bytesRead += read;
                }
            }
            return buffer;
        }
    }
}
