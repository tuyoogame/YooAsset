using System;
using System.IO;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建辅助工具类
    /// </summary>
    internal static class ArchiveFileBuildHelper
    {
        private const int StreamCopyBufferSize = 81920;

        /// <summary>
        /// 收集构建资源的归档条目列表，并按 FilePath 字典序排序
        /// </summary>
        public static List<ArchiveFileEntry> CollectEntries(IReadOnlyList<BuildAssetInfo> allAssets)
        {
            var entries = new List<ArchiveFileEntry>(allAssets.Count);
            foreach (var asset in allAssets)
            {
                string assetPath = asset.AssetInfo.AssetPath;
                long dataLength = new FileInfo(assetPath).Length;
                uint crc = HashUtility.ComputeFileCrc32AsUInt(assetPath);
                entries.Add(new ArchiveFileEntry(assetPath, dataLength, crc));
            }

            entries.Sort((a, b) => string.Compare(a.AssetPath, b.AssetPath, StringComparison.Ordinal));
            return entries;
        }

        /// <summary>
        /// 计算每个条目的绝对偏移，并写入归档文件
        /// </summary>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="entries">归档条目列表</param>
        /// <param name="fileAlignment">文件数据对齐字节数（0 表示不对齐）</param>
        public static void BuildArchiveFile(string outputPath, List<ArchiveFileEntry> entries, int fileAlignment)
        {
            int fileCount = entries.Count;
            if (fileCount > ArchiveBundleConsts.MaxChildFileCount)
                throw new InvalidOperationException($"Archive child file count {fileCount} exceeds maximum ({ArchiveBundleConsts.MaxChildFileCount}).");

            // 1. 计算 header 总大小
            int headerSize = 4 + 4 + 4; // Magic + Version + FileCount
            foreach (var entry in entries)
            {
                byte[] pathBytes = entry.GetPathBytes();
                // FilePathLen(4) + FilePath(变长) + DataOffset(8) + DataLength(8) + FileCRC(4)
                headerSize += 4 + pathBytes.Length + 8 + 8 + 4;
            }

            // 2. 计算每个文件的绝对偏移
            long currentOffset = headerSize;
            foreach (var entry in entries)
            {
                if (fileAlignment > 0)
                    currentOffset = AlignOffset(currentOffset, fileAlignment);
                entry.DataOffset = currentOffset;
                currentOffset += entry.DataLength;
            }

            // 3. 写入归档文件
            EditorFileUtility.CreateFileDirectory(outputPath);
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                // Header
                writer.Write(ArchiveBundleConsts.FileMagic);
                writer.Write(ArchiveBundleConsts.FileVersion);
                writer.Write(fileCount);

                foreach (var entry in entries)
                {
                    byte[] pathBytes = entry.GetPathBytes();
                    writer.Write(pathBytes.Length);
                    writer.Write(pathBytes);
                    writer.Write(entry.DataOffset);
                    writer.Write(entry.DataLength);
                    writer.Write(entry.FileCRC);
                }

                // Data: 按排序后的顺序写入，使用流式拷贝避免大文件 OOM
                byte[] buffer = new byte[StreamCopyBufferSize];
                foreach (var entry in entries)
                {
                    // 填充对齐字节
                    long paddingSize = entry.DataOffset - fs.Position;
                    if (paddingSize > 0)
                        writer.Write(new byte[paddingSize]);

                    using (var sourceStream = new FileStream(entry.AssetPath, FileMode.Open, FileAccess.Read))
                    {
                        int bytesRead;
                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writer.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将偏移值向上对齐到指定字节边界
        /// </summary>
        private static long AlignOffset(long offset, int alignment)
        {
            return (offset + alignment - 1) / alignment * alignment;
        }
    }
}
