namespace YooAsset
{
    /// <summary>
    /// WebGL 平台网络缓存系统
    /// </summary>
    internal class WebNetworkBundleCache : IBundleCache
    {
        internal readonly struct Configuration
        {
            /// <summary>
            /// 禁用 Unity 内置网络缓存
            /// </summary>
            public bool DisableUnityWebCache { get; }

            /// <summary>
            /// 下载数据校验级别
            /// </summary>
            public EFileVerifyLevel DownloadVerifyLevel { get; }

            /// <summary>
            /// AssetBundle 解密器
            /// </summary>
            public IBundleDecryptor AssetBundleDecryptor { get; }

            /// <summary>
            /// RawBundle 解密器
            /// </summary>
            public IBundleDecryptor RawBundleDecryptor { get; }

            /// <summary>
            /// ArchiveBundle 解密器
            /// </summary>
            public IBundleDecryptor ArchiveBundleDecryptor { get; }

            /// <summary>
            /// 平台策略接口
            /// </summary>
            public IWebPlatformStrategy PlatformStrategy { get; }

            /// <summary>
            /// 远程服务接口
            /// </summary>
            public IRemoteService RemoteService { get; }

            /// <summary>
            /// 下载后台接口
            /// </summary>
            public IDownloadBackend DownloadBackend { get; }

            /// <summary>
            /// 下载重试判定策略
            /// </summary>
            public IDownloadRetryPolicy DownloadRetryPolicy { get; }

            /// <summary>
            /// URL 选择策略
            /// </summary>
            public IDownloadUrlPolicy DownloadUrlPolicy { get; }

            public Configuration(bool disableUnityWebCache,
               EFileVerifyLevel downloadVerifyLevel, IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor,
               IBundleDecryptor archiveBundleDecryptor, IWebPlatformStrategy platformStrategy, IRemoteService remoteService,
               IDownloadBackend downloadBackend, IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
            {
                DisableUnityWebCache = disableUnityWebCache;
                DownloadVerifyLevel = downloadVerifyLevel;
                AssetBundleDecryptor = assetBundleDecryptor;
                RawBundleDecryptor = rawBundleDecryptor;
                ArchiveBundleDecryptor = archiveBundleDecryptor;
                PlatformStrategy = platformStrategy;
                RemoteService = remoteService;
                DownloadBackend = downloadBackend;
                DownloadRetryPolicy = downloadRetryPolicy;
                DownloadUrlPolicy = downloadUrlPolicy;
            }
        }

        /// <summary>
        /// 缓存配置
        /// </summary>
        internal readonly Configuration Config;

        #region 接口属性
        /// <inheritdoc/>
        public string PackageName { get; }

        /// <inheritdoc/>
        public string RootPath { get; }

        /// <inheritdoc/>
        public bool IsReadOnly { get; }

        /// <inheritdoc/>
        public int FileCount { get; }

        /// <inheritdoc/>
        public long SpaceOccupied { get; }
        #endregion

        public WebNetworkBundleCache(string packageName, Configuration config)
        {
            PackageName = packageName;
            Config = config;
            IsReadOnly = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
        /// <inheritdoc />
        public BCInitializeOperation InitializeAsync()
        {
            var operation = new WNBCInitializeOperation();
            return operation;
        }
        /// <inheritdoc />
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options)
        {
            var operation = new BCWriteCacheCompleteOperation($"{nameof(WebNetworkBundleCache)} is readonly.");
            return operation;
        }
        /// <inheritdoc />
        public BCClearCacheOperation ClearCacheAsync(BCClearCacheOptions options)
        {
            var operation = new BCClearCacheCompleteOperation();
            return operation;
        }
        /// <inheritdoc />
        public BCVerifyCacheOperation VerifyCacheAsync(BCVerifyCacheOptions options)
        {
            var operation = new BCVerifyCacheCompleteOperation();
            return operation;
        }
        /// <inheritdoc />
        public BCLoadBundleOperation LoadBundleAsync(BCLoadBundleOptions options)
        {
            if (options.Bundle.GetBundleType() == (int)EBundleType.AssetBundle)
            {
                var operation = new WNBCLoadAssetBundleOperation(this, options);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.RawBundle)
            {
                var operation = new WNBCLoadRawBundleOperation(this, options);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.ArchiveBundle)
            {
                var operation = new WNBCLoadArchiveBundleOperation(this, options);
                return operation;
            }
            else
            {
                string error = $"{nameof(WebNetworkBundleCache)} does not support bundle type: {options.Bundle.GetBundleType()}.";
                var operation = new BCLoadBundleErrorOperation(error);
                return operation;
            }
        }
        /// <inheritdoc />
        public bool IsCached(string bundleGuid)
        {
            return true;
        }
        /// <inheritdoc />
        public string GetCacheFilePath(string bundleGuid)
        {
            YooLogger.LogWarning($"{nameof(WebNetworkBundleCache)} does not support local cache file path.");
            return null;
        }
    }
}
