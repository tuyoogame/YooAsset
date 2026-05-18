using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存系统，用于编辑器模式下的资源模拟加载。
    /// </summary>
    internal class EditorBundleCache : IBundleCache
    {
        internal readonly struct Configuration
        {
            /// <summary>
            /// 虚拟下载模式，模拟资源下载流程。
            /// </summary>
            public bool VirtualDownloadMode { get; }

            /// <summary>
            /// 虚拟WebGL模式
            /// </summary>
            public bool VirtualWebGLMode { get; }

            /// <summary>
            /// 异步模拟最小帧数
            /// </summary>
            public int AsyncSimulateMinFrame { get; }

            /// <summary>
            /// 异步模拟最大帧数
            /// </summary>
            public int AsyncSimulateMaxFrame { get; }

            public Configuration(bool virtualDownloadMode, bool virtualWebGLMode, int asyncSimulateMinFrame, int asyncSimulateMaxFrame)
            {
                VirtualDownloadMode = virtualDownloadMode;
                VirtualWebGLMode = virtualWebGLMode;
                AsyncSimulateMinFrame = asyncSimulateMinFrame;
                AsyncSimulateMaxFrame = asyncSimulateMaxFrame;
            }
        }

        private readonly Dictionary<string, EditorBundleCacheEntry> _cacheEntries = new Dictionary<string, EditorBundleCacheEntry>(10000);

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
        /// 创建 EditorBundleCache 实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="rootPath">缓存根目录</param>
        /// <param name="config">缓存配置</param>
        public EditorBundleCache(string packageName, string rootPath, Configuration config)
        {
            PackageName = packageName;
            RootPath = rootPath;
            Config = config;
            IsReadOnly = config.VirtualDownloadMode == false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
        /// <inheritdoc />
        public BCInitializeOperation InitializeAsync()
        {
            var operation = new EBCInitializeOperation(this);
            return operation;
        }
        /// <inheritdoc />
        public BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options)
        {
            var operation = new EBCWriteCacheOperation(this, options);
            return operation;
        }
        /// <inheritdoc />
        public BCClearCacheOperation ClearCacheAsync(BCClearCacheOptions options)
        {
            ICacheEvictionPolicy policy = CreateEvictionPolicy(options);
            if (policy == null)
                return new BCClearCacheCompleteOperation($"Invalid clear method: '{options.ClearMethod}'.");

            return new EBCClearCacheOperation(this, options, policy);
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
            if (options.Bundle.GetBundleType() == (int)EBundleType.VirtualAssetBundle)
            {
                var operation = new EBCLoadVirtualAssetBundleOperation(this, options.Bundle);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.VirtualRawBundle)
            {
                var operation = new EBCLoadVirtualRawBundleOperation(this, options.Bundle);
                return operation;
            }
            else if (options.Bundle.GetBundleType() == (int)EBundleType.VirtualArchiveBundle)
            {
                var operation = new EBCLoadVirtualArchiveBundleOperation(this, options.Bundle);
                return operation;
            }
            else
            {
                string error = $"{nameof(EditorBundleCache)} does not support bundle type: {options.Bundle.GetBundleType()}.";
                var operation = new BCLoadBundleErrorOperation(error);
                return operation;
            }
        }
        /// <inheritdoc />
        public bool IsCached(string bundleGuid)
        {
            if (Config.VirtualDownloadMode == false)
                return true;

            return _cacheEntries.ContainsKey(bundleGuid);
        }
        /// <inheritdoc />
        public string GetCacheFilePath(string bundleGuid)
        {
            YooLogger.LogWarning($"{nameof(EditorBundleCache)} does not support local cache file path.");
            return null;
        }

        /// <summary>
        /// 根据 ClearMethod 创建对应的淘汰策略实例
        /// </summary>
        /// <param name="options">清理缓存选项</param>
        /// <returns>淘汰策略实例</returns>
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
        /// 获取所有缓存条目
        /// </summary>
        /// <returns>当前字典中全部缓存条目的只读集合</returns>
        internal IReadOnlyCollection<EditorBundleCacheEntry> GetAllEntries()
        {
            return _cacheEntries.Values;
        }

        /// <summary>
        /// 添加指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        /// <param name="cacheEntry">编辑器缓存条目</param>
        internal void AddEntry(string bundleGuid, EditorBundleCacheEntry cacheEntry)
        {
            if (_cacheEntries.ContainsKey(bundleGuid))
                throw new YooInternalException($"Cache entry already exists: '{bundleGuid}'.");

            _cacheEntries.Add(bundleGuid, cacheEntry);
        }

        /// <summary>
        /// 删除指定缓存条目
        /// </summary>
        /// <param name="bundleGuid">资源包 GUID</param>
        internal void RemoveEntry(string bundleGuid)
        {
            if (_cacheEntries.TryGetValue(bundleGuid, out EditorBundleCacheEntry entry))
            {
                _cacheEntries.Remove(bundleGuid);
                entry.Delete();
            }
        }

        /// <summary>
        /// 获取 marker 文件路径
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>marker 文件的完整路径</returns>
        internal string GetMarkerFilePath(PackageBundle bundle)
        {
            string bundleGuid = bundle.BundleGuid;
            string hashFolder = GetHashFolderName(bundleGuid);
            return PathUtility.Combine(RootPath, hashFolder, bundleGuid, EditorBundleCacheConsts.MarkerFileName);
        }

        /// <summary>
        /// 获取 marker 临时文件路径
        /// </summary>
        /// <param name="bundle">资源包描述</param>
        /// <returns>marker 临时文件的完整路径</returns>
        internal string GetMarkerTempFilePath(PackageBundle bundle)
        {
            string bundleGuid = bundle.BundleGuid;
            string hashFolder = GetHashFolderName(bundleGuid);
            return PathUtility.Combine(RootPath, hashFolder, bundleGuid, EditorBundleCacheConsts.MarkerTempFileName);
        }

        private string GetHashFolderName(string bundleGuid)
        {
            if (string.IsNullOrEmpty(bundleGuid))
                throw new YooInternalException("Bundle GUID is null or empty.");

            if (bundleGuid.Length <= EditorBundleCacheConsts.HashFolderNameLength)
                return bundleGuid;
            return bundleGuid.Substring(0, EditorBundleCacheConsts.HashFolderNameLength);
        }
        #endregion
    }
}
