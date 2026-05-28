using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存系统，用于管理下载到本地的资源包缓存。
    /// </summary>
    internal class SandboxBundleCache : IBundleCache
    {
        internal readonly struct Configuration
        {
            /// <summary>
            /// 文件校验最大并发数
            /// </summary>
            public int FileVerifyMaxConcurrency { get; }

            /// <summary>
            /// 文件校验级别
            /// </summary>
            public EFileVerifyLevel FileVerifyLevel { get; }

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
            /// AssetBundle 备用解密器
            /// </summary>
            public IBundleMemoryDecryptor AssetBundleFallbackDecryptor { get; }

            public Configuration(int fileVerifyMaxConcurrency, EFileVerifyLevel fileVerifyLevel,
                IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor,
                IBundleDecryptor archiveBundleDecryptor, IBundleMemoryDecryptor assetBundleFallbackDecryptor)
            {
                FileVerifyMaxConcurrency = fileVerifyMaxConcurrency;
                FileVerifyLevel = fileVerifyLevel;
                AssetBundleDecryptor = assetBundleDecryptor;
                RawBundleDecryptor = rawBundleDecryptor;
                ArchiveBundleDecryptor = archiveBundleDecryptor;
                AssetBundleFallbackDecryptor = assetBundleFallbackDecryptor;
            }
        }

        private readonly Dictionary<string, SandboxBundleCacheEntry> _cacheEntries = new Dictionary<string, SandboxBundleCacheEntry>(10000);
        private readonly Dictionary<string, string> _dataFilePathMapping = new Dictionary<string, string>(10000);
        private readonly Dictionary<string, string> _infoFilePathMapping = new Dictionary<string, string>(10000);

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
        /// 创建 SandboxBundleCache 实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="rootPath">缓存根目录</param>
        /// <param name="config">缓存配置</param>
        public SandboxBundleCache(string packageName, string rootPath, Configuration config)
        {
            PackageName = packageName;
            RootPath = rootPath;
            Config = config;
            IsReadOnly = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
        /// <inheritdoc />
        public BCInitializeOperation InitializeAsync()
        {
            var operation = new SBCInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options)
        {
            var operation = new SBCWriteCacheOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public BCClearCacheOperation ClearCacheAsync(BCClearCacheOptions options)
        {
            ICacheEvictionPolicy policy = CreateEvictionPolicy(options);
            if (policy == null)
                return new BCClearCacheCompleteOperation($"Invalid clear method: '{options.ClearMethod}'.");

            return new SBCClearCacheOperation(this, options, policy);
        }
        /// <inheritdoc />
        public BCVerifyCacheOperation VerifyCacheAsync(BCVerifyCacheOptions options)
        {
            var operation = new SBCVerifyCacheOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public BCLoadBundleOperation LoadBundleAsync(BCLoadBundleOptions options)
        {
            if (options.Bundle.GetBundleType() == (int)EBundleType.AssetBundle)
            {
                var operation = new SBCLoadAssetBundleOperation(this, options.Bundle);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.RawBundle)
            {
                var operation = new SBCLoadRawBundleOperation(this, options.Bundle);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.ArchiveBundle)
            {
                var operation = new SBCLoadArchiveBundleOperation(this, options.Bundle);
                return operation;
            }
            else
            {
                string error = $"{nameof(SandboxBundleCache)} does not support bundle type: {options.Bundle.GetBundleType()}.";
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
            if (_cacheEntries.TryGetValue(bundleGuid, out SandboxBundleCacheEntry entry))
            {
                return entry.DataFilePath;
            }

            YooLogger.LogWarning($"Cache file path not found. Bundle GUID: '{bundleGuid}'.");
            return null;
        }

        /// <summary>
        /// 根据 ClearMethod 创建对应的淘汰策略实例
        /// </summary>
        /// <param name="options">清理缓存选项</param>
        /// <returns>与清理方式匹配的淘汰策略实例</returns>
        protected ICacheEvictionPolicy CreateEvictionPolicy(BCClearCacheOptions options)
        {
            if (options.ClearMethod == ClearCacheMethods.ClearAllBundleFiles)
                return new EvictionAllPolicy();
            if (options.ClearMethod == ClearCacheMethods.ClearUnusedBundleFiles)
                return new EvictionUnusedPolicy();
            if (options.ClearMethod == ClearCacheMethods.ClearBundleFilesByLocations)
                return new EvictionByLocationsPolicy();
            if (options.ClearMethod == ClearCacheMethods.ClearBundleFilesByTags)
                return new EvictionByTagsPolicy();

            if (options.ClearParameter is ICacheEvictionPolicy customPolicy)
                return customPolicy;

            return null;
        }

        #region 内部方法
        /// <summary>
        /// 获取 Bundle 数据文件路径
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>数据文件的完整路径</returns>
        internal string GetDataFilePath(PackageBundle bundle)
        {
            string bundleGuid = bundle.BundleGuid;
            if (_dataFilePathMapping.TryGetValue(bundleGuid, out string filePath) == false)
            {
                string folderName = GetHashFolderName(bundleGuid);
                filePath = PathUtility.Combine(RootPath, folderName, bundleGuid, SandboxBundleCacheConsts.BundleDataFileName);
                _dataFilePathMapping.Add(bundleGuid, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 获取 Bundle 信息文件路径
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>信息文件的完整路径</returns>
        internal string GetInfoFilePath(PackageBundle bundle)
        {
            string bundleGuid = bundle.BundleGuid;
            if (_infoFilePathMapping.TryGetValue(bundleGuid, out string filePath) == false)
            {
                string folderName = GetHashFolderName(bundleGuid);
                filePath = PathUtility.Combine(RootPath, folderName, bundleGuid, SandboxBundleCacheConsts.BundleInfoFileName);
                _infoFilePathMapping.Add(bundleGuid, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 获取 Bundle 数据临时文件路径
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>数据临时文件的完整路径</returns>
        internal string GetDataTempFilePath(PackageBundle bundle)
        {
            string bundleGuid = bundle.BundleGuid;
            string folderName = GetHashFolderName(bundleGuid);
            return PathUtility.Combine(RootPath, folderName, bundleGuid, SandboxBundleCacheConsts.BundleDataTempFileName);
        }

        /// <summary>
        /// 获取 Bundle 信息临时文件路径
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>信息临时文件的完整路径</returns>
        internal string GetInfoTempFilePath(PackageBundle bundle)
        {
            string bundleGuid = bundle.BundleGuid;
            string folderName = GetHashFolderName(bundleGuid);
            return PathUtility.Combine(RootPath, folderName, bundleGuid, SandboxBundleCacheConsts.BundleInfoTempFileName);
        }

        /// <summary>
        /// 获取指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <returns>对应的沙盒缓存条目</returns>
        internal SandboxBundleCacheEntry GetEntry(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out SandboxBundleCacheEntry entry))
                return entry;
            else
                return null;
        }

        /// <summary>
        /// 获取所有缓存条目
        /// </summary>
        /// <returns>当前字典中全部沙盒缓存条目的只读集合</returns>
        internal IReadOnlyCollection<SandboxBundleCacheEntry> GetAllEntries()
        {
            return _cacheEntries.Values;
        }

        /// <summary>
        /// 添加指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <param name="cacheEntry">沙盒缓存条目</param>
        internal void AddEntry(string bundleGuid, SandboxBundleCacheEntry cacheEntry)
        {
            if (_cacheEntries.ContainsKey(bundleGuid))
                throw new YooInternalException($"Cache entry already exists: '{bundleGuid}'.");

            _cacheEntries.Add(bundleGuid, cacheEntry);
            SpaceOccupied += cacheEntry.GetFileSize();
        }

        /// <summary>
        /// 删除指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        internal void RemoveEntry(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out SandboxBundleCacheEntry entry))
            {
                _cacheEntries.Remove(bundleGuid);
                _dataFilePathMapping.Remove(bundleGuid);
                _infoFilePathMapping.Remove(bundleGuid);
                SpaceOccupied -= entry.GetFileSize();
                if (SpaceOccupied < 0)
                    SpaceOccupied = 0;
                entry.Delete();
            }
        }

        private string GetHashFolderName(string bundleGuid)
        {
            if (string.IsNullOrEmpty(bundleGuid))
                throw new YooInternalException("Bundle GUID is null or empty.");

            if (bundleGuid.Length <= SandboxBundleCacheConsts.HashFolderNameLength)
                return bundleGuid;
            return bundleGuid.Substring(0, SandboxBundleCacheConsts.HashFolderNameLength);
        }
        #endregion
    }
}
