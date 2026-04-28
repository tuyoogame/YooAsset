using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 按资源地址清理缓存文件
    /// </summary>
    /// <remarks>
    /// ClearParameter 类型：string / array / list
    /// </remarks>
    internal class EvictionByLocationsPolicy : ICacheEvictionPolicy
    {
        /// <inheritdoc />
        public EvictionResult SelectEvictionTargets(IReadOnlyCollection<ICacheEntry> cacheEntries, BCClearCacheOptions options)
        {
            if (options.Manifest == null)
                return EvictionResult.CreateFailure("Active package manifest not found.");
            if (options.ClearParameter == null)
                return EvictionResult.CreateFailure("Clear param is null.");

            string[] locations;
            if (options.ClearParameter is string str)
                locations = new string[] { str };
            else if (options.ClearParameter is List<string> list)
                locations = list.ToArray();
            else if (options.ClearParameter is string[] array)
                locations = array;
            else
                return EvictionResult.CreateFailure($"Invalid clear param: {options.ClearParameter.GetType().FullName}");

            var bundleGuids = new List<string>(locations.Length);
            foreach (var location in locations)
            {
                string assetPath = options.Manifest.TryMappingToAssetPath(location);
                if (options.Manifest.TryGetPackageAsset(assetPath, out PackageAsset packageAsset))
                {
                    PackageBundle bundle = options.Manifest.GetMainPackageBundle(packageAsset.BundleID);
                    bundleGuids.Add(bundle.BundleGuid);
                }
            }
            return EvictionResult.CreateSuccess(bundleGuids);
        }
    }
}
