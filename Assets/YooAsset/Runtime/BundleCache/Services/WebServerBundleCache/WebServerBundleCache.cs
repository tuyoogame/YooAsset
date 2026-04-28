using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// Web服务器文件缓存系统，用于WebGL平台从服务器加载资源。
    /// </summary>
    internal class WebServerBundleCache : IBundleCache
    {
        internal readonly struct Configuration
        {
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
            /// 下载后台
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

            public Configuration(int watchdogTimeout, bool disableUnityWebCache, 
                EFileVerifyLevel downloadVerifyLevel, IBundleDecryptor assetBundleDecryptor, 
                IDownloadBackend downloadBackend, IDownloadRetryPolicy downloadRetryPolicy, IDownloadUrlPolicy downloadUrlPolicy)
            {
                WatchdogTimeout = watchdogTimeout;
                DisableUnityWebCache = disableUnityWebCache;
                DownloadVerifyLevel = downloadVerifyLevel;
                AssetBundleDecryptor = assetBundleDecryptor;
                DownloadBackend = downloadBackend;
                DownloadRetryPolicy = downloadRetryPolicy;
                DownloadUrlPolicy = downloadUrlPolicy;
            }
        }

        private readonly Dictionary<string, WebServerBundleCacheEntry> _cacheEntries = new Dictionary<string, WebServerBundleCacheEntry>(10000);

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
        public int FileCount
        {
            get
            {
                return _cacheEntries.Count;
            }
        }

        /// <inheritdoc/>
        public long SpaceOccupied { get; private set; }
        #endregion

        /// <summary>
        /// 创建 WebServerBundleCache 实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="rootPath">缓存根目录</param>
        /// <param name="config">缓存配置</param>
        public WebServerBundleCache(string packageName, string rootPath, Configuration config)
        {
            PackageName = packageName;
            RootPath = rootPath;
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
            var operation = new WSBCInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options)
        {
            var operation = new BCWriteCacheCompleteOperation($"{nameof(WebServerBundleCache)} is readonly.");
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
                var operation = new WSBCLoadAssetBundleOperation(this, options);
                return operation;
            }
            else
            {
                string error = $"{nameof(WebServerBundleCache)} does not support bundle type: {options.Bundle.GetBundleType()}.";
                var operation = new BCLoadBundleErrorOperation(error);
                return operation;
            }
        }
        /// <inheritdoc />
        public bool IsCached(string bundleGuid)
        {
            return _cacheEntries.ContainsKey(bundleGuid);
        }

        #region 内部方法
        /// <summary>
        /// 获取指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <returns>对应的 Web 服务器缓存条目</returns>
        internal WebServerBundleCacheEntry GetEntry(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out WebServerBundleCacheEntry entry))
                return entry;
            else
                return null;
        }

        /// <summary>
        /// 添加指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <param name="cacheEntry">Web 服务器缓存条目</param>
        internal void AddEntry(string bundleGuid, WebServerBundleCacheEntry cacheEntry)
        {
            if (_cacheEntries.ContainsKey(bundleGuid))
                throw new YooInternalException($"Cache entry already exists: '{bundleGuid}'.");

            _cacheEntries.Add(bundleGuid, cacheEntry);
        }

        /// <summary>
        /// 获取Catalog文件加载路径
        /// </summary>
        /// <returns>文件的完整加载路径</returns>
        internal string GetCatalogBinaryFileLoadPath()
        {
            return PathUtility.Combine(RootPath, BuiltinCatalogConsts.BinaryFileName);
        }
        #endregion
    }
}
