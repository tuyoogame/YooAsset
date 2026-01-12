using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 清理所有缓存文件
    /// </summary>
    internal class EvictionAllPolicy : ICacheEvictionPolicy
    {
        /// <inheritdoc />
        public EvictionResult SelectEvictionTargets(IReadOnlyCollection<ICacheEntry> cacheEntries, BCClearCacheOptions options)
        {
            var bundleGuids = new List<string>(cacheEntries.Count);
            foreach (var entry in cacheEntries)
            {
                bundleGuids.Add(entry.BundleGuid);
            }
            return EvictionResult.CreateSuccess(bundleGuids);
        }
    }
}
