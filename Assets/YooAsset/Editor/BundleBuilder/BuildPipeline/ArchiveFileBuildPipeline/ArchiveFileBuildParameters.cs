using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的构建参数
    /// </summary>
    public class ArchiveFileBuildParameters : BuildParameters
    {
        private const int MaxFileAlignment = 4096;

        /// <summary>
        /// 文件哈希值计算包含路径信息
        /// </summary>
        public bool IncludePathInHash { get; set; } = false;

        /// <summary>
        /// 归档文件内数据对齐字节数（0 表示不对齐）
        /// </summary>
        /// <remarks>
        /// 对齐后每个子文件的数据偏移会向上取整到该值的整数倍，文件间以零字节填充。
        /// 推荐值：4（通用对齐）、512（磁盘扇区对齐）、4096（内存页对齐）。
        /// </remarks>
        public int FileAlignment { get; set; } = 0;


        /// <inheritdoc />
        protected override void CheckBuildParametersCore()
        {
            // 校验文件对齐参数范围
            if (FileAlignment < 0 || FileAlignment > MaxFileAlignment)
            {
                throw new ArgumentOutOfRangeException(nameof(FileAlignment),
                    $"FileAlignment must be between 0 and {MaxFileAlignment}. Current value: {FileAlignment}.");
            }
        }
    }
}
