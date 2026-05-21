namespace YooAsset
{
    /// <summary>
    /// 加载Web远端包裹清单的操作选项
    /// </summary>
    internal readonly struct LoadWebPackageManifestOptions
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { get; }

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { get; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 远端资源地址查询服务
        /// </summary>
        public IRemoteService RemoteService { get; }

        /// <summary>
        /// 资源清单解密器
        /// </summary>
        public IManifestDecryptor ManifestDecryptor { get; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; }

        /// <summary>
        /// URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; }

        public LoadWebPackageManifestOptions(string packageName, string packageVersion, string packageHash,
            int timeout, IRemoteService remoteService, IManifestDecryptor manifestDecryptor,
            IDownloadBackend downloadBackend, IDownloadUrlPolicy downloadUrlPolicy)
        {
            PackageName = packageName;
            PackageVersion = packageVersion;
            PackageHash = packageHash;
            Timeout = timeout;
            RemoteService = remoteService;
            ManifestDecryptor = manifestDecryptor;
            DownloadBackend = downloadBackend;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
