namespace YooAsset
{
    /// <summary>
    /// 请求Web远端包裹版本的操作选项
    /// </summary>
    internal readonly struct RequestWebPackageVersionOptions
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 在URL末尾添加时间戳
        /// </summary>
        public bool AppendTimeTicks { get; }

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

        public RequestWebPackageVersionOptions(string packageName, bool appendTimeTicks, int timeout,
            IRemoteService remoteService, IDownloadBackend downloadBackend, IDownloadUrlPolicy downloadUrlPolicy)
        {
            PackageName = packageName;
            AppendTimeTicks = appendTimeTicks;
            Timeout = timeout;
            RemoteService = remoteService;
            DownloadBackend = downloadBackend;
            DownloadUrlPolicy = downloadUrlPolicy;
        }
    }
}
