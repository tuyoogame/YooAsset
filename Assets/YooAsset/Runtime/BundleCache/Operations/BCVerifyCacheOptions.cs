
namespace YooAsset
{
    /// <summary>
    /// 验证缓存的操作选项
    /// </summary>
    internal readonly struct BCVerifyCacheOptions
    {
        /// <summary>
        /// 要验证的资源包
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 失败后直接移除缓存条目
        /// </summary>
        public bool DeleteCacheEntryOnFailure { get; }

        public BCVerifyCacheOptions(PackageBundle bundle, bool deleteCacheEntryOnFailure)
        {
            Bundle = bundle;
            DeleteCacheEntryOnFailure = deleteCacheEntryOnFailure;
        }
    }
}
