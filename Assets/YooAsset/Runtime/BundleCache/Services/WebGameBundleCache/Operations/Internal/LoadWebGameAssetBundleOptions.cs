using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台加载 AssetBundle 的配置选项
    /// </summary>
    internal readonly struct LoadWebGameAssetBundleOptions
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
        /// 游戏平台接口
        /// </summary>
        public IWebGamePlatform GamePlatform { get; }

        /// <summary>
        /// 平台侧缓存文件路径
        /// </summary>
        public string CacheFilePath { get; }

        /// <summary>
        /// 看门狗超时时间
        /// </summary>
        public int WatchdogTimeout { get; }

        /// <summary>
        /// 下载重试判定策略
        /// </summary>
        public IDownloadRetryPolicy DownloadRetryPolicy { get; }

        /// <summary>
        /// URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; }

        public LoadWebGameAssetBundleOptions(PackageBundle bundle, IReadOnlyList<string> candidateUrls,
           IWebGamePlatform gamePlatform, string cacheFilePath, int watchdogTimeout,
            IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
        {
            Bundle = bundle;
            CandidateUrls = candidateUrls;
            GamePlatform = gamePlatform;
            CacheFilePath = cacheFilePath;
            WatchdogTimeout = watchdogTimeout;
            DownloadRetryPolicy = downloadRetryPolicy;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
