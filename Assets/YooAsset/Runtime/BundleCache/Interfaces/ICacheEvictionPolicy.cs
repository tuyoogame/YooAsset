using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 缓存淘汰策略接口
    /// </summary>
    internal interface ICacheEvictionPolicy
    {
        /// <summary>
        /// 选出需要清理的 BundleGuid 列表
        /// </summary>
        /// <param name="cacheEntries">当前全部缓存条目的只读集合</param>
        /// <param name="options">缓存清理的配置参数</param>
        /// <returns>包含待清理 BundleGuid 列表的执行结果，失败时携带错误信息。</returns>
        EvictionResult SelectEvictionTargets(IReadOnlyCollection<ICacheEntry> cacheEntries, BCClearCacheOptions options);
    }
}
