using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 淘汰策略执行结果
    /// </summary>
    internal readonly struct EvictionResult
    {
        private readonly bool _initialized;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// 需要清理的资源标识符集合
        /// </summary>
        public IReadOnlyList<string> BundleGuids { get; }

        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool Succeeded
        {
            get { return _initialized && Error == null; }
        }

        private EvictionResult(string error, IReadOnlyList<string> bundleGuids)
        {
            _initialized = true;
            Error = error;
            BundleGuids = bundleGuids;
        }

        /// <summary>
        /// 创建表示执行成功的淘汰结果
        /// </summary>
        /// <param name="bundleGuids">需要清理的资源包标识符列表</param>
        /// <returns>携带待清理列表的成功结果</returns>
        public static EvictionResult CreateSuccess(IReadOnlyList<string> bundleGuids)
        {
            return new EvictionResult(null, bundleGuids);
        }

        /// <summary>
        /// 创建表示执行失败的淘汰结果
        /// </summary>
        /// <param name="error">描述失败原因的错误信息</param>
        /// <returns>携带错误信息的失败结果</returns>
        public static EvictionResult CreateFailure(string error)
        {
            return new EvictionResult(error, null);
        }
    }

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
