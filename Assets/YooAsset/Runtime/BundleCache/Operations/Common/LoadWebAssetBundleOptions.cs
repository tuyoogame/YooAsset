using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 加载 AssetBundle 的上下文信息
    /// </summary>
    internal readonly struct LoadWebAssetBundleOptions
    {
        /// <summary>
        /// 文件缓存名称
        /// </summary>
        public string CacheName { get; }

        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 候选下载地址列表
        /// </summary>
        public IReadOnlyList<string> CandidateUrls { get; }

        /// <summary>
        /// AssetBundle 解密器
        /// </summary>
        public IBundleDecryptor AssetBundleDecryptor { get; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; }

        /// <summary>
        /// 下载数据校验级别
        /// </summary>
        public EFileVerifyLevel DownloadVerifyLevel { get; }

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

        public LoadWebAssetBundleOptions(string cacheName, PackageBundle bundle, IReadOnlyList<string> candidateUrls,
            IBundleDecryptor assetBundleDecryptor, IDownloadBackend downloadBackend, EFileVerifyLevel downloadVerifyLevel,
            int watchdogTimeout, bool disableUnityWebCache, IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
        {
            CacheName = cacheName;
            Bundle = bundle;
            CandidateUrls = candidateUrls;
            AssetBundleDecryptor = assetBundleDecryptor;
            DownloadBackend = downloadBackend;
            DownloadVerifyLevel = downloadVerifyLevel;
            WatchdogTimeout = watchdogTimeout;
            DisableUnityWebCache = disableUnityWebCache;
            DownloadRetryPolicy = downloadRetryPolicy;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
