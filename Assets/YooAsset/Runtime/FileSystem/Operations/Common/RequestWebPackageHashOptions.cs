namespace YooAsset
{
    /// <summary>
    /// 请求Web远端包裹哈希的操作选项
    /// </summary>
    internal readonly struct RequestWebPackageHashOptions
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
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 远端资源地址查询服务
        /// </summary>
        public IRemoteService RemoteService { get; }

        /// <summary>
        /// 下载后台接口
        /// </summary>
        public IDownloadBackend DownloadBackend { get; }

        /// <summary>
        /// URL 选择策略
        /// </summary>
        public IDownloadUrlPolicy DownloadUrlPolicy { get; }

        public RequestWebPackageHashOptions(string packageName, string packageVersion, int timeout,
            IRemoteService remoteService, IDownloadBackend downloadBackend, IDownloadUrlPolicy downloadUrlPolicy)
        {
            PackageName = packageName;
            PackageVersion = packageVersion;
            Timeout = timeout;
            RemoteService = remoteService;
            DownloadBackend = downloadBackend;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
