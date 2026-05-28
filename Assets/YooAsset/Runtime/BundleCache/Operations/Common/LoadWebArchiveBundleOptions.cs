using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// WebGL 平台加载 ArchiveBundle 的操作选项
    /// </summary>
    internal readonly struct LoadWebArchiveBundleOptions
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
        /// ArchiveBundle 解密器
        /// </summary>
        public IBundleDecryptor ArchiveBundleDecryptor { get; }

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
        /// 下载重试判定策略
        /// </summary>
        public IDownloadRetryPolicy DownloadRetryPolicy { get; }

        /// <summary>
        /// URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; }

        public LoadWebArchiveBundleOptions(string cacheName, PackageBundle bundle, IReadOnlyList<string> candidateUrls,
            IBundleDecryptor archiveBundleDecryptor, IDownloadBackend downloadBackend, EFileVerifyLevel downloadVerifyLevel,
            int watchdogTimeout, IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
        {
            CacheName = cacheName;
            Bundle = bundle;
            CandidateUrls = candidateUrls;
            ArchiveBundleDecryptor = archiveBundleDecryptor;
            DownloadBackend = downloadBackend;
            DownloadVerifyLevel = downloadVerifyLevel;
            WatchdogTimeout = watchdogTimeout;
            DownloadRetryPolicy = downloadRetryPolicy;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
