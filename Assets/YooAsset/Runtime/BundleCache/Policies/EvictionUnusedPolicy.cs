using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 清理 Manifest 中未引用的缓存文件
    /// </summary>
    internal class EvictionUnusedPolicy : ICacheEvictionPolicy
    {
        /// <inheritdoc />
        public EvictionResult SelectEvictionTargets(IReadOnlyCollection<ICacheEntry> cacheEntries, BCClearCacheOptions options)
        {
            if (options.Manifest == null)
                return EvictionResult.CreateFailure("Active package manifest not found.");

            var bundleGuids = new List<string>(cacheEntries.Count);
            foreach (var entry in cacheEntries)
            {
                if (options.Manifest.ContainsBundle(entry.BundleGuid) == false)
                {
                    bundleGuids.Add(entry.BundleGuid);
                }
            }
            return EvictionResult.CreateSuccess(bundleGuids);
        }
    }
}
