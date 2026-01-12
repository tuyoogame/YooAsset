
namespace YooAsset
{
    /// <summary>
    /// 清单文件常量定义
    /// </summary>
    internal static class PackageManifestConsts
    {
        /// <summary>
        /// 文件极限大小（100MB）
        /// </summary>
        public const int MaxFileSize = 104857600;

        /// <summary>
        /// 文件头标识
        /// </summary>
        public const uint FileMagic = 0x594F4F;

        /// <summary>
        /// 文件版本号
        /// </summary>
        public const int FileVersion = 1;

        /// <summary>
        /// 文件最小合法大小（37字节）
        /// </summary>
        public const int MinFileSize = 37;
    }
}