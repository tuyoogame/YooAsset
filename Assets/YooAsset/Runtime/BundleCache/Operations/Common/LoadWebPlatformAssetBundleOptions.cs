using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// WebGL 平台加载非加密 AssetBundle 的操作选项
    /// </summary>
    internal readonly struct LoadWebPlatformAssetBundleOptions
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 候选下载地址列表
        /// </summary>
        public IReadOnlyList<string> CandidateUrls { get; }

        /// <summary>
        /// 平台策略
        /// </summary>
        public IWebPlatformStrategy PlatformStrategy { get; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; }

        /// <summary>
        /// 看门狗超时时间
        /// </summary>
        public int WatchdogTimeout { get; }

        /// <summary>
        /// 禁用 Unity 内置网络缓存
        /// </summary>
        public bool DisableUnityWebCache { get; }

        /// <summary>
        /// 下载重试判定策略
        /// </summary>
        public IDownloadRetryPolicy DownloadRetryPolicy { get; }

        /// <summary>
        /// URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; }

        public LoadWebPlatformAssetBundleOptions(PackageBundle bundle, IReadOnlyList<string> candidateUrls,
            IWebPlatformStrategy platformStrategy, IDownloadBackend downloadBackend, int watchdogTimeout, bool disableUnityWebCache,
            IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
        {
            Bundle = bundle;
            CandidateUrls = candidateUrls;
            PlatformStrategy = platformStrategy;
            DownloadBackend = downloadBackend;
            WatchdogTimeout = watchdogTimeout;
            DisableUnityWebCache = disableUnityWebCache;
            DownloadRetryPolicy = downloadRetryPolicy;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
