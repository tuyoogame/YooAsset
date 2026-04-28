using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台缓存系统
    /// </summary>
    internal class WebGameBundleCache : IBundleCache
    {
        internal readonly struct Configuration
        {
            /// <summary>
            /// 游戏平台接口
            /// </summary>
            public IWebGamePlatform GamePlatform { get; }

            /// <summary>
            /// 看门狗超时时间
            /// </summary>
            public int WatchdogTimeout { get; }

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

            public Configuration(IWebGamePlatform gamePlatform,
                int watchdogTimeout, bool disableUnityWebCache,
                EFileVerifyLevel downloadVerifyLevel, IBundleDecryptor assetBundleDecryptor, IRemoteService remoteService,
                IDownloadBackend downloadBackend, IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
            {
                GamePlatform = gamePlatform;
                WatchdogTimeout = watchdogTimeout;
                DisableUnityWebCache = disableUnityWebCache;
                DownloadVerifyLevel = downloadVerifyLevel;
                AssetBundleDecryptor = assetBundleDecryptor;
                RemoteService = remoteService;
                DownloadBackend = downloadBackend;
                DownloadRetryPolicy = downloadRetryPolicy;
                DownloadUrlPolicy = downloadUrlPolicy;
            }
        }

        private readonly Dictionary<string, WebGameBundleCacheEntry> _cacheEntries = new Dictionary<string, WebGameBundleCacheEntry>(10000);

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
        public int FileCount => _cacheEntries.Count;

        /// <inheritdoc/>
        public long SpaceOccupied => 0;
        #endregion

        /// <summary>
        /// 创建 WebGameBundleCache 实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="rootPath">缓存根目录</param>
        /// <param name="config">缓存配置</param>
        public WebGameBundleCache(string packageName, string rootPath, Configuration config)
        {
            PackageName = packageName;
            RootPath = rootPath;
            Config = config;
            IsReadOnly = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
        /// <inheritdoc/>
        public BCInitializeOperation InitializeAsync()
        {
            var operation = new WGBCInitializeOperation();
            return operation;
        }
        /// <inheritdoc/>
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options)
        {
            var operation = new BCWriteCacheCompleteOperation($"{nameof(WebGameBundleCache)} is readonly.");
            return operation;
        }
        /// <inheritdoc/>
        public BCClearCacheOperation ClearCacheAsync(BCClearCacheOptions options)
        {
            var operation = new BCClearCacheCompleteOperation();
            return operation;
        }
        /// <inheritdoc/>
        public BCVerifyCacheOperation VerifyCacheAsync(BCVerifyCacheOptions options)
        {
            var operation = new BCVerifyCacheCompleteOperation();
            return operation;
        }
        /// <inheritdoc/>
        public BCLoadBundleOperation LoadBundleAsync(BCLoadBundleOptions options)
        {
            if (options.Bundle.GetBundleType() == (int)EBundleType.AssetBundle)
            {
                var operation = new WGBCLoadAssetBundleOperation(this, options);
                return operation;
            }
            else
            {
                string error = $"{nameof(WebGameBundleCache)} does not support bundle type: {options.Bundle.GetBundleType()}.";
                var operation = new BCLoadBundleErrorOperation(error);
                return operation;
            }
        }
        /// <inheritdoc/>
        public bool IsCached(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out var entry))
                return Config.GamePlatform.IsCached(entry.CacheFilePath);
            return false;
        }

        #region 内部方法
        /// <summary>
        /// 获取或创建指定资源包的缓存条目
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>已存在则返回对应条目；否则创建并登记后返回新条目。</returns>
        internal WebGameBundleCacheEntry GetEntry(PackageBundle bundle)
        {
            if (_cacheEntries.TryGetValue(bundle.BundleGuid, out var entry))
                return entry;

            var urls = Config.RemoteService.GetRemoteUrls(bundle.GetFileName());
            var filePath = Config.GamePlatform.GetCacheFilePath(RootPath, bundle);
            var newEntry = new WebGameBundleCacheEntry(bundle.BundleGuid, urls, filePath);
            _cacheEntries.Add(bundle.BundleGuid, newEntry);
            return newEntry;
        }
        #endregion
    }
}
