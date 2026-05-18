
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存常量定义
    /// </summary>
    internal static class SandboxBundleCacheConsts
    {
        /// <summary>
        /// 数据文件名称
        /// </summary>
        public const string BundleDataFileName = "__data";

        /// <summary>
        /// 信息文件名称
        /// </summary>
        public const string BundleInfoFileName = "__info";

        /// <summary>
        /// 数据临时文件名称
        /// </summary>
        public const string BundleDataTempFileName = "__data.tmp";

        /// <summary>
        /// 信息临时文件名称
        /// </summary>
        public const string BundleInfoTempFileName = "__info.tmp";

        /// <summary>
        /// 信息文件头标识 (ASCII "YOC1")
        /// </summary>
        public const uint InfoFileMagic = 0x31434F59;

        /// <summary>
        /// 信息文件版本号
        /// </summary>
        public const int InfoFileVersion = 1;

        /// <summary>
        /// 信息文件预期大小（字节）
        /// </summary>
        public const int InfoFileExpectedSize = 36;

        /// <summary>
        /// 哈希分片目录前缀长度
        /// </summary>
        public const int HashFolderNameLength = 2;
    }
}