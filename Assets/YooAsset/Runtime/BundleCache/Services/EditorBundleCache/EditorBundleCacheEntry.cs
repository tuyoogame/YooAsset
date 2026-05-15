
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存条目
    /// </summary>
    internal class EditorBundleCacheEntry : ICacheEntry
    {
        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        public string BundleGuid { get; private set; }

        /// <summary>
        /// 创建编辑器文件缓存条目实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        public EditorBundleCacheEntry(string bundleGuid)
        {
            BundleGuid = bundleGuid;
        }
    }
}