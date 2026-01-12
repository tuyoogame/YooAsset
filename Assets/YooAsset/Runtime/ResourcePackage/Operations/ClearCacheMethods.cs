
namespace YooAsset
{
    /// <summary>
    /// 缓存清理方式标识符
    /// </summary>
    public static class ClearCacheMethods
    {
        /// <summary>
        /// 清理所有资源包文件
        /// </summary>
        public const string ClearAllBundleFiles = nameof(ClearAllBundleFiles);

        /// <summary>
        /// 清理未在使用的资源包文件
        /// </summary>
        public const string ClearUnusedBundleFiles = nameof(ClearUnusedBundleFiles);

        /// <summary>
        /// 清理指定地址的资源包文件
        /// </summary>
        public const string ClearBundleFilesByLocations = nameof(ClearBundleFilesByLocations);

        /// <summary>
        /// 清理指定标签的资源包文件
        /// </summary>
        public const string ClearBundleFilesByTags = nameof(ClearBundleFilesByTags);

        /// <summary>
        /// 清理所有清单文件
        /// </summary>
        public const string ClearAllManifestFiles = nameof(ClearAllManifestFiles);

        /// <summary>
        /// 清理未在使用的清单文件
        /// </summary>
        public const string ClearUnusedManifestFiles = nameof(ClearUnusedManifestFiles);
    }
}
