using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YooAsset
{
    internal static class ArchiveBundleHelper
    {
        /// <summary>
        /// 从本地文件解析 YARK 归档
        /// </summary>
        /// <param name="filePath">归档文件路径</param>
        /// <returns>解析成功的 ArchiveBundle 实例</returns>
        public static ArchiveBundle LoadFromFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var entries = ParseEntries(fs, fs.Length);
                return new ArchiveBundle(filePath, entries);
            }
        }

        /// <summary>
        /// 从解密后的内存数据解析 YARK 归档
        /// </summary>
        /// <param name="fileData">解密后的完整归档字节数据</param>
        /// <returns>解析成功的 ArchiveBundle 实例</returns>
        public static ArchiveBundle LoadFromMemory(byte[] fileData)
        {
            using (var ms = new MemoryStream(fileData, false))
            {
                var entries = ParseEntries(ms, fileData.Length);
                return new ArchiveBundle(fileData, entries);
            }
        }

        private static Dictionary<string, ArchiveBundle.FileEntry> ParseEntries(Stream stream, long dataLength)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                // 校验文件头魔数（YARK）
                uint magic = reader.ReadUInt32();
                if (magic != ArchiveBundleConsts.FileMagic)
                    throw new InvalidOperationException($"Invalid archive file magic: 0x{magic:X8}, expected 0x{ArchiveBundleConsts.FileMagic:X8}.");

                // 校验文件版本号
                int version = reader.ReadInt32();
                if (version != ArchiveBundleConsts.FileVersion)
                    throw new InvalidOperationException($"Unsupported archive file version: {version}, expected {ArchiveBundleConsts.FileVersion}.");

                // 读取子文件索引表
                int fileCount = reader.ReadInt32();
                if (fileCount < 0)
                    throw new InvalidOperationException($"Invalid archive file count: {fileCount}.");
                if (fileCount > ArchiveBundleConsts.MaxChildFileCount)
                    throw new InvalidOperationException($"Archive child file count {fileCount} exceeds maximum ({ArchiveBundleConsts.MaxChildFileCount}).");

                var entries = new Dictionary<string, ArchiveBundle.FileEntry>(fileCount);
                for (int i = 0; i < fileCount; i++)
                {
                    // 校验路径字节长度
                    int pathLen = reader.ReadInt32();
                    if (pathLen <= 0)
                        throw new InvalidOperationException($"Invalid path length {pathLen} at entry index {i}.");
                    if (pathLen > ArchiveBundleConsts.MaxChildFilePathBytes)
                        throw new InvalidOperationException($"Path length {pathLen} exceeds maximum ({ArchiveBundleConsts.MaxChildFilePathBytes}) at entry index {i}.");
                    long remaining = dataLength - stream.Position;
                    if (pathLen > remaining)
                        throw new InvalidOperationException($"Path length {pathLen} exceeds remaining data size at entry index {i}.");

                    string assetPath = Encoding.UTF8.GetString(reader.ReadBytes(pathLen));
                    if (string.IsNullOrEmpty(assetPath))
                        throw new InvalidOperationException($"Empty asset path at entry index {i}.");
                    if (entries.ContainsKey(assetPath))
                        throw new InvalidOperationException($"Duplicate asset path '{assetPath}' at entry index {i}.");

                    long offset = reader.ReadInt64();
                    long length = reader.ReadInt64();
                    uint crc = reader.ReadUInt32();

                    // 校验数据范围是否越过文件边界
                    if (offset < 0 || offset > dataLength)
                        throw new InvalidOperationException($"Invalid data offset {offset} for '{assetPath}'.");
                    if (length < 0 || length > dataLength - offset)
                        throw new InvalidOperationException($"Data range [{offset}, {offset + length}) exceeds data size {dataLength} for '{assetPath}'.");

                    entries[assetPath] = new ArchiveBundle.FileEntry(assetPath, offset, length, crc);
                }

                return entries;
            }
        }
    }
}
