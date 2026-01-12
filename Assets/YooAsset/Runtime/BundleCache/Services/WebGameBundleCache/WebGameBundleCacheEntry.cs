using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台缓存条目
    /// </summary>
    internal class WebGameBundleCacheEntry : ICacheEntry
    {
        /// <summary>
        /// 资源包唯一标识
        /// </summary>
        public string BundleGuid { get; }

        /// <summary>
        /// 候选下载地址列表
        /// </summary>
        public IReadOnlyList<string> Urls { get; }

        /// <summary>
        /// 平台侧缓存文件路径
        /// </summary>
        public string CacheFilePath { get; }

        /// <summary>
        /// 创建 WebGL 游戏平台缓存条目实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="urls">候选下载地址列表</param>
        /// <param name="cacheFilePath">平台侧缓存文件路径</param>
        public WebGameBundleCacheEntry(string bundleGuid, IReadOnlyList<string> urls, string cacheFilePath)
        {
            BundleGuid = bundleGuid;
            Urls = urls;
            CacheFilePath = cacheFilePath;
        }
    }
}
