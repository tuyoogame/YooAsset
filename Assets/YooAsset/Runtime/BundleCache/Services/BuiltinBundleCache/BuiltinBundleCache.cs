using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 内置文件缓存系统，用于管理 StreamingAssets 中的资源包。
    /// </summary>
    internal class BuiltinBundleCache : IBundleCache
    {
        internal readonly struct Configuration
        {
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
            /// 下载后台
            /// </summary>
            public IDownloadBackend DownloadBackend { get; }

            public Configuration(IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor,
                IBundleDecryptor archiveBundleDecryptor, IDownloadBackend downloadBackend)
            {
                AssetBundleDecryptor = assetBundleDecryptor;
                RawBundleDecryptor = rawBundleDecryptor;
                ArchiveBundleDecryptor = archiveBundleDecryptor;
                DownloadBackend = downloadBackend;
            }
        }

        private readonly Dictionary<string, BuiltinBundleCacheEntry> _cacheEntries = new Dictionary<string, BuiltinBundleCacheEntry>(10000);

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
        /// 创建 BuiltinBundleCache 实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="rootPath">缓存根目录</param>
        /// <param name="config">缓存配置</param>
        public BuiltinBundleCache(string packageName, string rootPath, Configuration config)
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
            var operation = new BBCInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options)
        {
            var operation = new BCWriteCacheCompleteOperation($"{nameof(BuiltinBundleCache)} is readonly.");
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
                var operation = new BBCLoadAssetBundleOperation(this, options.Bundle);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.RawBundle)
            {
                var operation = new BBCLoadRawBundleOperation(this, options.Bundle);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.ArchiveBundle)
            {
                var operation = new BBCLoadArchiveBundleOperation(this, options.Bundle);
                return operation;
            }
            else
            {
                string error = $"{nameof(BuiltinBundleCache)} does not support bundle type: {options.Bundle.GetBundleType()}.";
                var operation = new BCLoadBundleErrorOperation(error);
                return operation;
            }
        }
        /// <inheritdoc />
        public bool IsCached(string bundleGuid)
        {
            return _cacheEntries.ContainsKey(bundleGuid);
        }
        /// <inheritdoc />
        public string GetCacheFilePath(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out BuiltinBundleCacheEntry entry))
            {
                return entry.FilePath;
            }

            YooLogger.LogWarning($"Cache file path not found. Bundle GUID: '{bundleGuid}'.");
            return null;
        }

        #region 内部方法
        /// <summary>
        /// 获取指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <returns>对应的内置缓存条目</returns>
        internal BuiltinBundleCacheEntry GetEntry(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out BuiltinBundleCacheEntry entry))
                return entry;
            else
                return null;
        }

        /// <summary>
        /// 添加指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <param name="cacheEntry">内置缓存条目</param>
        internal void AddEntry(string bundleGuid, BuiltinBundleCacheEntry cacheEntry)
        {
            if (_cacheEntries.ContainsKey(bundleGuid))
                throw new YooInternalException($"Cache entry already exists: '{bundleGuid}'.");

            _cacheEntries.Add(bundleGuid, cacheEntry);
        }

        /// <summary>
        /// 获取Catalog文件加载路径
        /// </summary>
        /// <returns>完整加载路径</returns>
        internal string GetCatalogBinaryFileLoadPath()
        {
            return PathUtility.Combine(RootPath, BuiltinCatalogConsts.BinaryFileName);
        }
        #endregion
    }
}
