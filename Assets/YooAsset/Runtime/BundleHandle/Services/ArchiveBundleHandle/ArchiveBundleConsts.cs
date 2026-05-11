
namespace YooAsset
{
    /// <summary>
    /// 归档文件资源包的常量定义
    /// </summary>
    internal static class ArchiveBundleConsts
    {
        /// <summary>
        /// 文件头标识 "YARK"
        /// </summary>
        public const uint FileMagic = 0x5941524B;

        /// <summary>
        /// 文件版本号
        /// </summary>
        public const int FileVersion = 1;

        /// <summary>
        /// 单个归档资源包允许包含的最大文件数量
        /// </summary>
        public const int MaxChildFileCount = 65535;

        /// <summary>
        /// 子文件的最大字节长度（512 MB）
        /// </summary>
        /// <remarks>
        /// 构建期和运行时共用此上限，超过该值则不允许打入归档包。
        /// </remarks>
        public const long MaxChildFileSize = 512L * 1024 * 1024;

        /// <summary>
        /// 子文件路径的最大字节长度
        /// </summary>
        public const int MaxChildFilePathBytes = 4096;
    }
}
