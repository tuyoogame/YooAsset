using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 按标签清理缓存文件
    /// </summary>
    /// <remarks>
    /// ClearParameter 类型：string / array / list
    /// </remarks>
    internal class EvictionByTagsPolicy : ICacheEvictionPolicy
    {
        /// <inheritdoc />
        public EvictionResult SelectEvictionTargets(IReadOnlyCollection<ICacheEntry> cacheEntries, BCClearCacheOptions options)
        {
            if (options.Manifest == null)
                return EvictionResult.CreateFailure("Active package manifest not found.");
            if (options.ClearParameter == null)
                return EvictionResult.CreateFailure("Clear param is null.");

            string[] tags;
            if (options.ClearParameter is string str)
                tags = new string[] { str };
            else if (options.ClearParameter is List<string> list)
                tags = list.ToArray();
            else if (options.ClearParameter is string[] array)
                tags = array;
            else
                return EvictionResult.CreateFailure($"Invalid clear param: {options.ClearParameter.GetType().FullName}");

            var bundleGuids = new List<string>(cacheEntries.Count);
            foreach (var entry in cacheEntries)
            {
                if (options.Manifest.TryGetPackageBundleByBundleGuid(entry.BundleGuid, out PackageBundle bundle))
                {
                    if (bundle.HasAnyTag(tags))
                        bundleGuids.Add(bundle.BundleGuid);
                }
            }
            return EvictionResult.CreateSuccess(bundleGuids);
        }
    }
}
