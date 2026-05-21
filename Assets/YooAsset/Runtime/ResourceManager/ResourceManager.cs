using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 资源管理器，负责管理资源的加载、卸载和句柄创建。
    /// </summary>
    /// <remarks>
    /// 此类不是线程安全的，所有方法必须在 Unity 主线程调用。
    /// </remarks>
    internal class ResourceManager
    {
        private readonly Dictionary<string, ProviderBase> _providerDict = new Dictionary<string, ProviderBase>(5000);
        private readonly Dictionary<string, LoadBundleOperation> _bundleLoaderDict = new Dictionary<string, LoadBundleOperation>(5000);
        private readonly List<SceneHandle> _sceneHandles = new List<SceneHandle>(100);
        private readonly List<SceneHandle> _tempSceneHandles = new List<SceneHandle>(100);
        private readonly List<LoadBundleOperation> _tempRemoveLoaders = new List<LoadBundleOperation>(100);
        private FileSystemHost _fileSystemHost;
        private int _bundleLoadingMaxConcurrency;
        private int _bundleLoadingCounter;
        private long _sceneInstanceCounter;

        /// <summary>
        /// 当资源句柄引用计数为零时，是否自动卸载对应的资源包。
        /// </summary>
        public bool AutoUnloadBundleWhenUnused { get; private set; }

        /// <summary>
        /// WebGL 平台是否强制同步加载资源
        /// </summary>
        public bool WebGLForceSyncLoadAsset { get; private set; }

        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 加载操作是否被锁定
        /// </summary>
        public bool IsLoadingLocked { get; set; } = false;


        /// <summary>
        /// 创建资源管理器实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        public ResourceManager(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="options">初始化操作选项</param>
        /// <param name="host">文件系统宿主</param>
        public void Initialize(InitializePackageOptions options, FileSystemHost host)
        {
            _fileSystemHost = host;
            _bundleLoadingMaxConcurrency = options.BundleLoadingMaxConcurrency;
            AutoUnloadBundleWhenUnused = options.AutoUnloadBundleWhenUnused;
            WebGLForceSyncLoadAsset = options.WebGLForceSyncLoadAsset;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public void Destroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            ForceDestroyAllBundleLoaders();
        }

        /// <summary>
        /// 尝试卸载指定资源的资源包（包括依赖资源）
        /// </summary>
        /// <param name="assetInfo">要卸载的资源信息</param>
        /// <param name="loopCount">循环尝试次数，用于处理复杂依赖链。</param>
        public void TryUnloadUnusedAsset(AssetInfo assetInfo, int loopCount)
        {
            if (assetInfo == null)
            {
                YooLogger.LogError($"{nameof(AssetInfo)} is null.");
                return;
            }
            if (assetInfo.IsValid == false)
            {
                YooLogger.LogError($"Failed to unload asset. Error: {assetInfo.Error}");
                return;
            }

            // 多次循环尝试卸载，以处理复杂的依赖链
            // 例如：A依赖B，B依赖C，需要多次循环才能完全卸载
            while (loopCount > 0)
            {
                loopCount--;
                bool hasUnloaded = false;

                // 卸载主资源包加载器
                string mainBundleName = _fileSystemHost.GetMainBundleName(assetInfo.Asset.BundleID);
                var mainLoader = GetBundleLoader(mainBundleName);
                if (mainLoader != null)
                {
                    mainLoader.TryDestroyProviders();
                    if (mainLoader.CanDestroyLoader())
                    {
                        mainLoader.DestroyLoader();
                        _bundleLoaderDict.Remove(mainBundleName);
                        hasUnloaded = true;
                    }
                }

                // 卸载依赖资源包加载器
                foreach (var dependID in assetInfo.Asset.DependentBundleIDs)
                {
                    string dependBundleName = _fileSystemHost.GetMainBundleName(dependID);
                    var dependLoader = GetBundleLoader(dependBundleName);
                    if (dependLoader != null)
                    {
                        if (dependLoader.CanDestroyLoader())
                        {
                            dependLoader.DestroyLoader();
                            _bundleLoaderDict.Remove(dependBundleName);
                            hasUnloaded = true;
                        }
                    }
                }

                // 如果本次循环没有卸载任何资源，提前退出
                if (hasUnloaded == false)
                    break;
            }
        }

        /// <summary>
        /// 加载场景对象
        /// </summary>
        /// <remarks>
        /// <para>返回的场景句柄是唯一的，每个场景句柄对应自己的场景提供者对象。</para>
        /// <para>业务逻辑层应该避免同时加载一个子场景。</para>
        /// </remarks>
        /// <param name="assetInfo">场景资源信息</param>
        /// <param name="loadSceneParams">场景加载参数</param>
        /// <param name="allowSceneActivation">是否允许场景加载完成后自动激活</param>
        /// <param name="priority">加载优先级</param>
        /// <returns>场景句柄</returns>
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation, uint priority)
        {
            if (IsLoadingLocked)
            {
                string error = $"Loading is currently locked. New load requests are rejected.";
                YooLogger.LogError(error);
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(error);
                return errorProvider.CreateHandle<SceneHandle>();
            }

            if (assetInfo.IsValid == false)
            {
                YooLogger.LogError($"Failed to load scene. Error: {assetInfo.Error}");
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(assetInfo.Error);
                return errorProvider.CreateHandle<SceneHandle>();
            }

            // 注意：同一个场景的ProviderKey每次加载都会变化
            string providerKey = $"{assetInfo.AssetKey}-{++_sceneInstanceCounter}";
            ProviderBase provider;
            {
                provider = new SceneProvider(this, providerKey, assetInfo, loadSceneParams, allowSceneActivation);
                provider.InitProviderDebugInfo();
                _providerDict.Add(providerKey, provider);
                AsyncOperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            var handle = provider.CreateHandle<SceneHandle>();
            handle.PackageName = PackageName;
            _sceneHandles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载优先级</param>
        /// <returns>资源句柄</returns>
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority)
        {
            if (IsLoadingLocked)
            {
                string error = $"Loading is currently locked. New load requests are rejected.";
                YooLogger.LogError(error);
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(error);
                return errorProvider.CreateHandle<AssetHandle>();
            }

            if (assetInfo.IsValid == false)
            {
                YooLogger.LogError($"Failed to load asset. Error: {assetInfo.Error}");
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(assetInfo.Error);
                return errorProvider.CreateHandle<AssetHandle>();
            }

            string providerKey = nameof(LoadAssetAsync) + assetInfo.AssetKey;
            ProviderBase provider = GetAssetProvider(providerKey);
            if (provider == null)
            {
                provider = new AssetProvider(this, providerKey, assetInfo);
                provider.InitProviderDebugInfo();
                _providerDict.Add(providerKey, provider);
                AsyncOperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AssetHandle>();
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载优先级</param>
        /// <returns>子资源句柄</returns>
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (IsLoadingLocked)
            {
                string error = $"Loading is currently locked. New load requests are rejected.";
                YooLogger.LogError(error);
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(error);
                return errorProvider.CreateHandle<SubAssetsHandle>();
            }

            if (assetInfo.IsValid == false)
            {
                YooLogger.LogError($"Failed to load sub assets. Error: {assetInfo.Error}");
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(assetInfo.Error);
                return errorProvider.CreateHandle<SubAssetsHandle>();
            }

            string providerKey = nameof(LoadSubAssetsAsync) + assetInfo.AssetKey;
            ProviderBase provider = GetAssetProvider(providerKey);
            if (provider == null)
            {
                provider = new SubAssetsProvider(this, providerKey, assetInfo);
                provider.InitProviderDebugInfo();
                _providerDict.Add(providerKey, provider);
                AsyncOperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<SubAssetsHandle>();
        }

        /// <summary>
        /// 加载所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载优先级</param>
        /// <returns>全资源句柄</returns>
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (IsLoadingLocked)
            {
                string error = $"Loading is currently locked. New load requests are rejected.";
                YooLogger.LogError(error);
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(error);
                return errorProvider.CreateHandle<AllAssetsHandle>();
            }

            if (assetInfo.IsValid == false)
            {
                YooLogger.LogError($"Failed to load all assets. Error: {assetInfo.Error}");
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(assetInfo.Error);
                return errorProvider.CreateHandle<AllAssetsHandle>();
            }

            string providerKey = nameof(LoadAllAssetsAsync) + assetInfo.AssetKey;
            ProviderBase provider = GetAssetProvider(providerKey);
            if (provider == null)
            {
                provider = new AllAssetsProvider(this, providerKey, assetInfo);
                provider.InitProviderDebugInfo();
                _providerDict.Add(providerKey, provider);
                AsyncOperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AllAssetsHandle>();
        }

        /// <summary>
        /// 加载资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载优先级</param>
        /// <returns>资源包文件句柄</returns>
        public BundleFileHandle LoadBundleFileAsync(AssetInfo assetInfo, uint priority)
        {
            if (IsLoadingLocked)
            {
                string error = $"Loading is currently locked. New load requests are rejected.";
                YooLogger.LogError(error);
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(error);
                return errorProvider.CreateHandle<BundleFileHandle>();
            }

            if (assetInfo.IsValid == false)
            {
                YooLogger.LogError($"Failed to load bundle file. Error: {assetInfo.Error}");
                ErrorProvider errorProvider = new ErrorProvider(this, assetInfo);
                errorProvider.SetCompletedWithError(assetInfo.Error);
                return errorProvider.CreateHandle<BundleFileHandle>();
            }

            string providerKey = nameof(LoadBundleFileAsync) + assetInfo.AssetKey;
            ProviderBase provider = GetAssetProvider(providerKey);
            if (provider == null)
            {
                provider = new BundleFileProvider(this, providerKey, assetInfo);
                provider.InitProviderDebugInfo();
                _providerDict.Add(providerKey, provider);
                AsyncOperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<BundleFileHandle>();
        }

        /// <summary>
        /// 获取或创建主资源包加载器
        /// </summary>
        internal LoadBundleOperation GetOrCreateMainBundleLoader(AssetInfo assetInfo)
        {
            BundleInfo bundleInfo = _fileSystemHost.GetMainBundleInfo(assetInfo);
            return GetOrCreateBundleLoader(bundleInfo);
        }

        /// <summary>
        /// 获取或创建依赖资源包加载器列表
        /// </summary>
        internal List<LoadBundleOperation> GetOrCreateDependBundleLoaders(AssetInfo assetInfo)
        {
            List<BundleInfo> bundleInfos = _fileSystemHost.GetDependentBundleInfos(assetInfo);
            List<LoadBundleOperation> result = new List<LoadBundleOperation>(bundleInfos.Count);
            foreach (var bundleInfo in bundleInfos)
            {
                var bundleLoader = GetOrCreateBundleLoader(bundleInfo);
                result.Add(bundleLoader);
            }
            return result;
        }

        /// <summary>
        /// 从字典中移除指定的资源提供者
        /// </summary>
        internal void RemoveBundleProviders(List<ProviderBase> removeList)
        {
            foreach (var provider in removeList)
            {
                _providerDict.Remove(provider.ProviderKey);
            }
        }

        /// <summary>
        /// 检查指定资源包是否已销毁
        /// </summary>
        internal bool CheckBundleDestroyed(int bundleID)
        {
            string bundleName = _fileSystemHost.GetMainBundleName(bundleID);
            var bundleFileLoader = GetBundleLoader(bundleName);
            if (bundleFileLoader == null)
                return true;
            return bundleFileLoader.IsDestroyed;
        }

        /// <summary>
        /// 检查指定资源包是否可以释放
        /// </summary>
        internal bool CheckBundleReleasable(int bundleID)
        {
            string bundleName = _fileSystemHost.GetMainBundleName(bundleID);
            var bundleFileLoader = GetBundleLoader(bundleName);
            if (bundleFileLoader == null)
                return true;
            return bundleFileLoader.IsReleasable();
        }

        /// <summary>
        /// 检查是否存在任何资源包加载器
        /// </summary>
        internal bool HasAnyLoader()
        {
            return _bundleLoaderDict.Count > 0;
        }

        /// <summary>
        /// 释放所有 Provider 持有的句柄
        /// </summary>
        internal void ReleaseAllHandles()
        {
            // 注意：创建快照是因为释放句柄可能触发 TryUnloadBundle 逻辑，进而修改字典容器。
            var snapshot = new List<ProviderBase>(_providerDict.Values);
            foreach (var provider in snapshot)
            {
                provider.ReleaseAllHandles();
            }
        }

        /// <summary>
        /// 检查是否所有 Provider 都已完成
        /// </summary>
        internal bool AreAllProvidersDone()
        {
            foreach (var provider in _providerDict.Values)
            {
                if (provider.IsDone == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 销毁所有 Provider 并清空字典
        /// </summary>
        internal void DestroyAllProviders()
        {
            foreach (var provider in _providerDict.Values)
            {
                provider.DestroyProvider();
            }
            _providerDict.Clear();
        }

        /// <summary>
        /// 向所有 Provider 下发强制销毁请求
        /// </summary>
        internal void RequestForceDestroyAllProviders()
        {
            foreach (var provider in _providerDict.Values)
            {
                provider.RequestForceDestroy();
            }
        }

        /// <summary>
        /// 向所有 BundleLoader 下发强制销毁请求
        /// </summary>
        internal void RequestForceDestroyAllBundleLoaders()
        {
            foreach (var loader in _bundleLoaderDict.Values)
            {
                loader.RequestForceDestroy();
            }
        }

        /// <summary>
        /// 销毁所有 BundleLoader 并清空字典
        /// </summary>
        internal void DestroyAllBundleLoaders()
        {
            foreach (var loader in _bundleLoaderDict.Values)
            {
                loader.DestroyLoader();
            }
            _bundleLoaderDict.Clear();
        }

        /// <summary>
        /// 强制销毁所有 BundleLoader 并清空字典（仅用于全局 Destroy 场景）
        /// </summary>
        internal void ForceDestroyAllBundleLoaders()
        {
            foreach (var loader in _bundleLoaderDict.Values)
            {
                loader.ForceDestroyLoader();
            }
            _bundleLoaderDict.Clear();
        }

        /// <summary>
        /// 销毁未使用的 Bundle
        /// </summary>
        internal void DestroyUnusedBundle()
        {
            // 注意：优先销毁资源提供者
            foreach (var loader in _bundleLoaderDict.Values)
            {
                loader.TryDestroyProviders();
            }

            // 获取销毁列表
            _tempRemoveLoaders.Clear();
            foreach (var loader in _bundleLoaderDict.Values)
            {
                if (loader.CanDestroyLoader())
                    _tempRemoveLoaders.Add(loader);
            }

            // 销毁文件加载器
            foreach (var loader in _tempRemoveLoaders)
            {
                loader.DestroyLoader();
                _bundleLoaderDict.Remove(loader.LoadBundleInfo.Bundle.BundleName);
            }
            _tempRemoveLoaders.Clear();
        }

        /// <summary>
        /// 清空场景句柄列表
        /// </summary>
        internal void ClearSceneHandles()
        {
            _sceneHandles.Clear();
        }

        /// <summary>
        /// 增加资源包加载计数器
        /// </summary>
        internal void IncrementBundleLoadingCounter()
        {
            _bundleLoadingCounter++;
        }

        /// <summary>
        /// 减少资源包加载计数器
        /// </summary>
        internal void DecrementBundleLoadingCounter()
        {
            _bundleLoadingCounter--;
            if (_bundleLoadingCounter < 0)
            {
                YooLogger.LogError("Bundle loading counter became negative and was reset to zero.");
                _bundleLoadingCounter = 0;
            }
        }

        /// <summary>
        /// 获取当前资源包加载计数
        /// </summary>
        internal int GetBundleLoadingCounter()
        {
            return _bundleLoadingCounter;
        }

        /// <summary>
        /// 检查资源包加载是否繁忙（达到并发上限）
        /// </summary>
        internal bool IsBundleLoadingBusy()
        {
            return _bundleLoadingCounter >= _bundleLoadingMaxConcurrency;
        }

        private LoadBundleOperation GetOrCreateBundleLoader(BundleInfo bundleInfo)
        {
            // 如果加载器已经存在
            string bundleName = bundleInfo.Bundle.BundleName;
            LoadBundleOperation loaderOperation = GetBundleLoader(bundleName);
            if (loaderOperation != null)
                return loaderOperation;

            // 新增下载需求
            loaderOperation = new LoadBundleOperation(this, bundleInfo);
            _bundleLoaderDict.Add(bundleName, loaderOperation);
            return loaderOperation;
        }
        private LoadBundleOperation GetBundleLoader(string bundleName)
        {
            if (_bundleLoaderDict.TryGetValue(bundleName, out LoadBundleOperation value))
                return value;
            else
                return null;
        }
        private ProviderBase GetAssetProvider(string providerKey)
        {
            if (_providerDict.TryGetValue(providerKey, out ProviderBase value))
                return value;
            else
                return null;
        }
        private void OnSceneUnloaded(Scene scene)
        {
            _tempSceneHandles.Clear();
            foreach (var sceneHandle in _sceneHandles)
            {
                if (sceneHandle.IsValid == false)
                {
                    _tempSceneHandles.Add(sceneHandle);
                    continue;
                }
                if (sceneHandle.SceneObject == scene)
                {
                    sceneHandle.Release();
                    _tempSceneHandles.Add(sceneHandle);
                }
            }
            foreach (var sceneHandle in _tempSceneHandles)
            {
                _sceneHandles.Remove(sceneHandle);
            }
        }

        #region 调试信息
        /// <summary>
        /// 获取所有资源提供者的调试信息
        /// </summary>
        internal List<DiagnosticProviderInfo> DebugGetProviderInfos()
        {
            List<DiagnosticProviderInfo> result = new List<DiagnosticProviderInfo>(_providerDict.Count);
            foreach (var provider in _providerDict.Values)
            {
                DiagnosticProviderInfo providerInfo = new DiagnosticProviderInfo();
                providerInfo.AssetPath = provider.MainAssetInfo.AssetPath;
                providerInfo.SceneName = provider.SceneName;
                providerInfo.StartTime = provider.StartTime;
                providerInfo.ElapsedMilliseconds = provider.ElapsedMilliseconds;
                providerInfo.ReferenceCount = provider.RefCount;
                providerInfo.Status = provider.Status.ToString();
                providerInfo.Dependencies = provider.GetDebugDependBundles();
                result.Add(providerInfo);
            }
            return result;
        }

        /// <summary>
        /// 获取所有资源包加载器的调试信息
        /// </summary>
        internal List<DiagnosticBundleInfo> DebugGetBundleInfos()
        {
            List<DiagnosticBundleInfo> result = new List<DiagnosticBundleInfo>(_bundleLoaderDict.Values.Count);
            foreach (var bundleLoader in _bundleLoaderDict.Values)
            {
                var packageBundle = bundleLoader.LoadBundleInfo.Bundle;
                var bundleInfo = new DiagnosticBundleInfo();
                bundleInfo.BundleName = packageBundle.BundleName;
                bundleInfo.ReferenceCount = bundleLoader.RefCount;
                bundleInfo.Status = bundleLoader.Status.ToString();
                bundleInfo.Referencers = FilterReferenceBundles(packageBundle);
                result.Add(bundleInfo);
            }
            return result;
        }

        /// <summary>
        /// 过滤出当前已加载的引用资源包
        /// </summary>
        private List<string> FilterReferenceBundles(PackageBundle packageBundle)
        {
            // 注意：引用的资源包不一定在内存中，所以需要过滤
            var referrerBundleNames = packageBundle.DebugGetReferrerBundleNames();
            List<string> result = new List<string>(referrerBundleNames.Count);
            foreach (var bundleName in referrerBundleNames)
            {
                if (_bundleLoaderDict.ContainsKey(bundleName))
                    result.Add(bundleName);
            }
            return result;
        }
        #endregion
    }
}