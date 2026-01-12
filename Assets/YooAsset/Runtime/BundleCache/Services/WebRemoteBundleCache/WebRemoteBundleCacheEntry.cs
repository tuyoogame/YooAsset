using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// Web远端文件缓存条目
    /// </summary>
    internal class WebRemoteBundleCacheEntry : ICacheEntry
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
        /// 创建 Web 远端文件缓存条目实例
        /// </summary>
        /// <param name="bundleGuid">资源包唯一标识</param>
        /// <param name="urls">候选下载地址列表</param>
        public WebRemoteBundleCacheEntry(string bundleGuid, IReadOnlyList<string> urls)
        {
            BundleGuid = bundleGuid;
            Urls = urls;
        }
    }
}
