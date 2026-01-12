
namespace YooAsset
{
    /// <summary>
    /// 内置文件缓存条目
    /// </summary>
    internal class BuiltinBundleCacheEntry : ICacheEntry
    {
        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        public string BundleGuid { get; private set; }

        /// <summary>
        /// 资源包文件路径
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 创建内置文件缓存条目实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="filePath">资源包文件路径</param>
        public BuiltinBundleCacheEntry(string bundleGuid, string filePath)
        {
            BundleGuid = bundleGuid;
            FilePath = filePath;
        }
    }
}