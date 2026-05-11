using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YooAsset
{
    internal static class ArchiveBundleHelper
    {
        /// <summary>
        /// 解析 YARK 归档文件
        /// </summary>
        /// <param name="filePath">归档文件路径</param>
        /// <returns>解析成功的 ArchiveBundle 实例</returns>
        public static ArchiveBundle LoadArchiveBundle(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(fs))
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

                long fileLength = fs.Length;
                var entries = new Dictionary<string, ArchiveBundle.FileEntry>(fileCount);
                for (int i = 0; i < fileCount; i++)
                {
                    // 校验路径字节长度
                    int pathLen = reader.ReadInt32();
                    if (pathLen <= 0)
                        throw new InvalidOperationException($"Invalid path length {pathLen} at entry index {i}.");
                    if (pathLen > ArchiveBundleConsts.MaxChildFilePathBytes)
                        throw new InvalidOperationException($"Path length {pathLen} exceeds maximum ({ArchiveBundleConsts.MaxChildFilePathBytes}) at entry index {i}.");
                    long remaining = fileLength - fs.Position;
                    if (pathLen > remaining)
                        throw new InvalidOperationException($"Path length {pathLen} exceeds remaining file size at entry index {i}.");

                    string assetPath = Encoding.UTF8.GetString(reader.ReadBytes(pathLen));
                    if (string.IsNullOrEmpty(assetPath))
                        throw new InvalidOperationException($"Empty asset path at entry index {i}.");
                    if (entries.ContainsKey(assetPath))
                        throw new InvalidOperationException($"Duplicate asset path '{assetPath}' at entry index {i}.");

                    long offset = reader.ReadInt64();
                    long length = reader.ReadInt64();
                    uint crc = reader.ReadUInt32();

                    // 校验数据范围是否越过文件边界
                    if (offset < 0 || offset > fileLength)
                        throw new InvalidOperationException($"Invalid data offset {offset} for '{assetPath}'.");
                    if (length < 0 || length > fileLength - offset)
                        throw new InvalidOperationException($"Data range [{offset}, {offset + length}) exceeds file size {fileLength} for '{assetPath}'.");

                    entries[assetPath] = new ArchiveBundle.FileEntry(assetPath, offset, length, crc);
                }

                return new ArchiveBundle(filePath, entries);
            }
        }
    }
}
