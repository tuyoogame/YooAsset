
namespace YooAsset.Editor
{
    /// <summary>
    /// 收集资源搜索结果
    /// </summary>
    public class CollectAssetSearchResult
    {
        /// <summary>
        /// 命中的收集器分组
        /// </summary>
        public BundleCollectorGroup Group { get; }

        /// <summary>
        /// 命中的收集器分组索引
        /// </summary>
        public int GroupIndex { get; }

        /// <summary>
        /// 命中的收集器
        /// </summary>
        public BundleCollector Collector { get; }

        /// <summary>
        /// 命中的收集器索引
        /// </summary>
        public int CollectorIndex { get; }

        /// <summary>
        /// 命中的资源路径
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 构建收集资源搜索结果
        /// </summary>
        /// <param name="group">命中的收集器分组</param>
        /// <param name="groupIndex">命中的收集器分组索引</param>
        /// <param name="collector">命中的收集器</param>
        /// <param name="collectorIndex">命中的收集器索引</param>
        /// <param name="assetPath">命中的资源路径</param>
        public CollectAssetSearchResult(BundleCollectorGroup group, int groupIndex,
            BundleCollector collector, int collectorIndex, string assetPath)
        {
            Group = group;
            GroupIndex = groupIndex;
            Collector = collector;
            CollectorIndex = collectorIndex;
            AssetPath = assetPath;
        }
    }
}