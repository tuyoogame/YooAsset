using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 资源包裹类
    /// </summary>
    public partial class ResourcePackage
    {
        private InitializePackageOperation _initializeOp;
        private ResourceManager _resourceManager;
        private FileSystemHost _fileSystemHost;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 包裹是否有效
        /// </summary>
        public bool PackageValid
        {
            get
            {
                if (_fileSystemHost == null)
                    return false;
                return _fileSystemHost.ActiveManifest != null;
            }
        }

        /// <summary>
        /// 包裹优先级（值越大越优先更新）
        /// </summary>
        public uint PackagePriority
        {
            get { return AsyncOperationSystem.GetSchedulerPriority(PackageName); }
            set { AsyncOperationSystem.SetSchedulerPriority(PackageName, value); }
        }

        /// <summary>
        /// 初始化状态
        /// </summary>
        public EOperationStatus InitializeStatus
        {
            get
            {
                if (_initializeOp == null)
                    return EOperationStatus.None;
                return _initializeOp.Status;
            }
        }

        internal ResourcePackage(string packageName)
        {
            PackageName = packageName;
        }
        internal void InternalInitialize(ResourceManager manager, FileSystemHost host)
        {
            _resourceManager = manager;
            _fileSystemHost = host;
        }
        internal void InternalDestroy()
        {
            _initializeOp = null;

            // 销毁资源管理器
            if (_resourceManager != null)
            {
                _resourceManager.Destroy();
                _resourceManager = null;
            }

            // 销毁文件系统中枢
            if (_fileSystemHost != null)
            {
                _fileSystemHost.Destroy();
                _fileSystemHost = null;
            }
        }

        /// <summary>
        /// 初始化包裹
        /// </summary>
        /// <param name="options">初始化参数</param>
        /// <returns>返回初始化操作对象</returns>
        public InitializePackageOperation InitializePackageAsync(InitializePackageOptions options)
        {
            if (options == null)
                throw new System.ArgumentNullException(nameof(options));

            // 注意：联机平台因为网络原因可能会初始化失败！
            TryResetAfterFailure();

            // 检测重复初始化
            if (_initializeOp != null)
                throw new System.InvalidOperationException($"Resource package '{PackageName}' is already initialized.");

            // 开始初始化操作
            _initializeOp = new InitializePackageOperation(this, options);
            AsyncOperationSystem.StartOperation(PackageName, _initializeOp);
            return _initializeOp;
        }
        private void TryResetAfterFailure()
        {
            if (InitializeStatus == EOperationStatus.Failed)
            {
                InternalDestroy();
            }
        }

        /// <summary>
        /// 销毁包裹
        /// </summary>
        /// <returns>返回销毁包裹操作对象</returns>
        public DestroyPackageOperation DestroyPackageAsync()
        {
            var options = new UnloadAllAssetsOptions(true, true);
            var operation = new DestroyPackageOperation(this, _resourceManager, options);
            AsyncOperationSystem.StartOperation(AsyncOperationSystem.GlobalSchedulerName, operation);
            return operation;
        }

        /// <summary>
        /// 请求最新的资源版本
        /// </summary>
        /// <returns>返回请求版本操作对象</returns>
        /// <remarks>超时时间默认60秒</remarks>
        public RequestPackageVersionOperation RequestPackageVersionAsync()
        {
            int defaultTimeout = 60;
            var options = new RequestPackageVersionOptions(true, defaultTimeout);
            return RequestPackageVersionAsync(options);
        }

        /// <summary>
        /// 请求最新的资源版本
        /// </summary>
        /// <param name="options">请求版本选项</param>
        /// <returns>返回请求版本操作对象</returns>
        public RequestPackageVersionOperation RequestPackageVersionAsync(RequestPackageVersionOptions options)
        {
            CheckInitialized(false);
            var operation = new RequestPackageVersionOperation(_fileSystemHost, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 加载指定版本的资源清单
        /// </summary>
        /// <param name="options">加载清单选项</param>
        /// <returns>返回加载清单操作对象</returns>
        public LoadPackageManifestOperation LoadPackageManifestAsync(LoadPackageManifestOptions options)
        {
            CheckInitialized(false);

            // 注意：强烈建议在更新之前保持加载器为空！
            if (_resourceManager.HasAnyLoader())
            {
                YooLogger.LogWarning($"Found loaded bundles before updating the manifest. It is recommended to call the {nameof(UnloadAllAssetsAsync)} method to release loaded bundles.");
            }

            var operation = new LoadPackageManifestOperation(_fileSystemHost, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 预取指定版本的包裹资源清单
        /// </summary>
        /// <param name="options">预取清单选项</param>
        /// <returns>返回预取清单操作对象</returns>
        public PrefetchManifestOperation PrefetchManifestAsync(PrefetchManifestOptions options)
        {
            CheckInitialized(false);
            var operation = new PrefetchManifestOperation(_fileSystemHost, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 清理缓存文件
        /// </summary>
        /// <param name="options">清理缓存选项</param>
        /// <returns>返回清理缓存操作对象</returns>
        public ClearCacheOperation ClearCacheAsync(ClearCacheOptions options)
        {
            CheckInitialized(false);
            var operation = new ClearCacheOperation(_fileSystemHost, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }


        #region 包裹信息
        /// <summary>
        /// 获取当前加载包裹的版本信息
        /// </summary>
        /// <returns>返回当前包裹版本字符串</returns>
        public string GetPackageVersion()
        {
            CheckInitialized();
            return _fileSystemHost.ActiveManifest.PackageVersion;
        }

        /// <summary>
        /// 获取当前加载包裹的备注信息
        /// </summary>
        /// <returns>返回当前包裹备注字符串</returns>
        public string GetPackageNote()
        {
            CheckInitialized();
            return _fileSystemHost.ActiveManifest.PackageNote;
        }

        /// <summary>
        /// 获取当前加载包裹的详细信息
        /// </summary>
        /// <returns>返回包含包裹配置的详细信息对象</returns>
        public PackageDetails GetPackageDetails()
        {
            CheckInitialized();
            return _fileSystemHost.ActiveManifest.GetPackageDetails();
        }
        #endregion

        #region 资源回收
        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <returns>返回卸载资源操作对象</returns>
        public UnloadAllAssetsOperation UnloadAllAssetsAsync()
        {
            var options = new UnloadAllAssetsOptions(true, true);
            return UnloadAllAssetsAsync(options);
        }

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <param name="options">卸载选项</param>
        /// <returns>返回卸载资源操作对象</returns>
        public UnloadAllAssetsOperation UnloadAllAssetsAsync(UnloadAllAssetsOptions options)
        {
            CheckInitialized();
            var operation = new UnloadAllAssetsOperation(_resourceManager, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 回收不再使用的资源
        /// </summary>
        /// <returns>返回卸载未使用资源操作对象</returns>
        /// <remarks>卸载引用计数为零的资源，默认循环10次。</remarks>
        public UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync()
        {
            int defaultMaxLoopCount = 10;
            var options = new UnloadUnusedAssetsOptions(defaultMaxLoopCount);
            return UnloadUnusedAssetsAsync(options);
        }

        /// <summary>
        /// 回收不再使用的资源
        /// </summary>
        /// <param name="options">卸载选项</param>
        /// <returns>返回卸载未使用资源操作对象</returns>
        /// <remarks>卸载引用计数为零的资源</remarks>
        public UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync(UnloadUnusedAssetsOptions options)
        {
            CheckInitialized();
            var operation = new UnloadUnusedAssetsOperation(_resourceManager, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 尝试卸载指定的未使用资源
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="loopCount">最大循环卸载次数</param>
        public void TryUnloadUnusedAsset(string location, int loopCount = 10)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            _resourceManager.TryUnloadUnusedAsset(assetInfo, loopCount);
        }

        /// <summary>
        /// 尝试卸载指定的未使用资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="loopCount">最大循环卸载次数</param>
        public void TryUnloadUnusedAsset(AssetInfo assetInfo, int loopCount = 10)
        {
            CheckInitialized();
            _resourceManager.TryUnloadUnusedAsset(assetInfo, loopCount);
        }
        #endregion

        #region 资源信息
        /// <summary>
        /// 获取指定资源需要下载的文件总大小
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回需要下载的字节数，0 表示不需要下载。</returns>
        public long GetDownloadSize(string location)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return _fileSystemHost.GetDownloadSize(assetInfo);
        }

        /// <summary>
        /// 获取指定资源需要下载的文件总大小
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns>返回需要下载的字节数，0 表示不需要下载。</returns>
        public long GetDownloadSize(AssetInfo assetInfo)
        {
            CheckInitialized();
            return _fileSystemHost.GetDownloadSize(assetInfo);
        }

        /// <summary>
        /// 获取所有的资源信息
        /// </summary>
        /// <returns>返回包含所有资源信息的数组</returns>
        public AssetInfo[] GetAllAssetInfos()
        {
            CheckInitialized();
            return _fileSystemHost.ActiveManifest.GetAllAssetInfos();
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <returns>返回匹配标签的资源信息数组</returns>
        public AssetInfo[] GetAssetInfos(string tag)
        {
            CheckInitialized();
            string[] tags = new string[] { tag };
            return _fileSystemHost.ActiveManifest.GetAssetInfosByTags(tags);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <returns>返回匹配标签的资源信息数组</returns>
        public AssetInfo[] GetAssetInfos(string[] tags)
        {
            CheckInitialized();
            return _fileSystemHost.ActiveManifest.GetAssetInfosByTags(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回资源信息对象</returns>
        public AssetInfo GetAssetInfo(string location)
        {
            CheckInitialized();
            return ConvertLocationToAssetInfo(location, null);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        /// <returns>返回资源信息对象</returns>
        public AssetInfo GetAssetInfo(string location, System.Type type)
        {
            CheckInitialized();
            return ConvertLocationToAssetInfo(location, type);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGuid">资源GUID</param>
        /// <returns>返回资源信息对象</returns>
        public AssetInfo GetAssetInfoByGuid(string assetGuid)
        {
            CheckInitialized();
            return ConvertAssetGuidToAssetInfo(assetGuid, null);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGuid">资源GUID</param>
        /// <param name="type">资源类型</param>
        /// <returns>返回资源信息对象</returns>
        public AssetInfo GetAssetInfoByGuid(string assetGuid, System.Type type)
        {
            CheckInitialized();
            return ConvertAssetGuidToAssetInfo(assetGuid, type);
        }

        /// <summary>
        /// 资源定位地址是否有效
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>如果地址有效返回true，否则返回false。</returns>
        public bool IsLocationValid(string location)
        {
            CheckInitialized();
            string assetPath = _fileSystemHost.ActiveManifest.TryMappingToAssetPath(location);
            return string.IsNullOrEmpty(assetPath) == false;
        }
        #endregion

        #region 场景加载
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <returns>返回场景操作句柄</returns>
        public SceneHandle LoadSceneSync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadSceneInternal(assetInfo, true, sceneMode, physicsMode, true, 0);
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <returns>返回场景操作句柄</returns>
        public SceneHandle LoadSceneSync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            CheckInitialized();
            return LoadSceneInternal(assetInfo, true, sceneMode, physicsMode, true, 0);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="allowSceneActivation">是否允许场景激活</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回场景操作句柄</returns>
        public SceneHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool allowSceneActivation = true, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadSceneInternal(assetInfo, false, sceneMode, physicsMode, allowSceneActivation, priority);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="allowSceneActivation">是否允许场景激活</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回场景操作句柄</returns>
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool allowSceneActivation = true, uint priority = 0)
        {
            CheckInitialized();
            return LoadSceneInternal(assetInfo, false, sceneMode, physicsMode, allowSceneActivation, priority);
        }

        private SceneHandle LoadSceneInternal(AssetInfo assetInfo, bool waitForAsyncComplete, LoadSceneMode sceneMode, LocalPhysicsMode physicsMode, bool allowSceneActivation, uint priority)
        {
            DebugValidateAssetType(assetInfo.AssetType);
            assetInfo.LoadMethod = ELoadMethod.LoadScene;
            var loadSceneParams = new LoadSceneParameters(sceneMode, physicsMode);
            var handle = _resourceManager.LoadSceneAsync(assetInfo, loadSceneParams, allowSceneActivation, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源加载 [主资源]
        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            CheckInitialized();
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetSync(string location, System.Type type)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetSync(string location)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(UnityEngine.Object));
            return LoadAssetInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority = 0)
        {
            CheckInitialized();
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetAsync(string location, System.Type type, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回资源操作句柄</returns>
        public AssetHandle LoadAssetAsync(string location, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(UnityEngine.Object));
            return LoadAssetInternal(assetInfo, false, priority);
        }


        private AssetHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugValidateAssetType(assetInfo.AssetType);
            assetInfo.LoadMethod = ELoadMethod.LoadAsset;
            var handle = _resourceManager.LoadAssetAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源加载 [子资源]
        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            CheckInitialized();
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsSync(string location, System.Type type)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsSync(string location)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(UnityEngine.Object));
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            CheckInitialized();
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsAsync(string location, System.Type type, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回子资源操作句柄</returns>
        public SubAssetsHandle LoadSubAssetsAsync(string location, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(UnityEngine.Object));
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }


        private SubAssetsHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugValidateAssetType(assetInfo.AssetType);
            assetInfo.LoadMethod = ELoadMethod.LoadSubAssets;
            var handle = _resourceManager.LoadSubAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源加载 [全资源]
        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            CheckInitialized();
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsSync(string location, System.Type type)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsSync(string location)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(UnityEngine.Object));
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            CheckInitialized();
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsAsync(string location, System.Type type, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回全资源操作句柄</returns>
        public AllAssetsHandle LoadAllAssetsAsync(string location, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(UnityEngine.Object));
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }


        private AllAssetsHandle LoadAllAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugValidateAssetType(assetInfo.AssetType);
            assetInfo.LoadMethod = ELoadMethod.LoadAllAssets;
            var handle = _resourceManager.LoadAllAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源包文件
        /// <summary>
        /// 确保资源包文件已就绪
        /// </summary>
        /// <param name="options">确保资源包文件已就绪的选项</param>
        /// <returns>返回确保资源包文件就绪的操作对象</returns>
        public EnsureBundleFileOperation EnsureBundleFileAsync(EnsureBundleFileOptions options)
        {
            CheckInitialized();
            if (options.AssetInfo == null)
            {
                AssetInfo assetInfo = ConvertLocationToAssetInfo(options.Location, null);
                options = new EnsureBundleFileOptions(assetInfo);
            }

            var operation = new EnsureBundleFileOperation(_fileSystemHost, options);
            AsyncOperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 同步加载资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns>返回资源包文件操作句柄</returns>
        public BundleFileHandle LoadBundleFileSync(AssetInfo assetInfo)
        {
            CheckInitialized();
            return LoadBundleFileInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <returns>返回资源包文件操作句柄</returns>
        public BundleFileHandle LoadBundleFileSync(string location)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadBundleFileInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 异步加载资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回资源包文件操作句柄</returns>
        public BundleFileHandle LoadBundleFileAsync(AssetInfo assetInfo, uint priority = 0)
        {
            CheckInitialized();
            return LoadBundleFileInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        /// <returns>返回资源包文件操作句柄</returns>
        public BundleFileHandle LoadBundleFileAsync(string location, uint priority = 0)
        {
            CheckInitialized();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadBundleFileInternal(assetInfo, false, priority);
        }

        private BundleFileHandle LoadBundleFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            assetInfo.LoadMethod = ELoadMethod.LoadBundleFile;
            var handle = _resourceManager.LoadBundleFileAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源下载
        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件。
        /// </summary>
        /// <param name="options">资源下载选项</param>
        /// <returns>返回资源下载操作对象</returns>
        public ResourceDownloaderOperation CreateResourceDownloader(ResourceDownloaderOptions options)
        {
            CheckInitialized();
            return _fileSystemHost.CreateResourceDownloader(options);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源信息列表依赖的资源包文件。
        /// </summary>
        /// <param name="options">资源下载选项</param>
        /// <returns>返回资源下载操作对象</returns>
        public ResourceDownloaderOperation CreateResourceDownloader(BundleDownloaderOptions options)
        {
            CheckInitialized();
            return _fileSystemHost.CreateResourceDownloader(options);
        }
        #endregion

        #region 资源解压
        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签关联的资源包文件。
        /// </summary>
        /// <param name="options">资源解压选项</param>
        /// <returns>返回资源解压操作对象</returns>
        public ResourceUnpackerOperation CreateResourceUnpacker(ResourceUnpackerOptions options)
        {
            CheckInitialized();
            return _fileSystemHost.CreateResourceUnpacker(options);
        }
        #endregion

        #region 资源导入
        /// <summary>
        /// 创建资源导入器
        /// </summary>
        /// <param name="options">资源导入选项</param>
        /// <returns>返回资源导入操作对象</returns>
        public ResourceImporterOperation CreateResourceImporter(BundleImporterOptions options)
        {
            CheckInitialized();
            return _fileSystemHost.CreateResourceImporter(options);
        }
        #endregion

        #region 内部方法
        internal AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
        {
            return _fileSystemHost.ActiveManifest.ConvertLocationToAssetInfo(location, assetType);
        }
        internal AssetInfo ConvertAssetGuidToAssetInfo(string assetGuid, System.Type assetType)
        {
            return _fileSystemHost.ActiveManifest.ConvertAssetGuidToAssetInfo(assetGuid, assetType);
        }
        #endregion

        #region 调试方法
        private void CheckInitialized(bool checkActiveManifest = true)
        {
            if (InitializeStatus != EOperationStatus.Succeeded)
            {
                switch (InitializeStatus)
                {
                    case EOperationStatus.None:
                        throw new YooPackageInvalidException(PackageName, "Resource package not initialized.");

                    case EOperationStatus.Processing:
                        throw new YooPackageInvalidException(PackageName, "Resource package initialization not completed.");

                    case EOperationStatus.Failed:
                        string error = _initializeOp == null ? string.Empty : _initializeOp.Error;
                        throw new YooPackageInvalidException(PackageName, $"Resource package initialization failed. Error: {error}");
                }
            }

            if (checkActiveManifest)
            {
                if (_fileSystemHost.ActiveManifest == null)
                    throw new YooPackageInvalidException(PackageName, "Active package manifest not found.");
            }
        }

        [Conditional("DEBUG")]
        private void DebugValidateAssetType(System.Type type)
        {
            if (type == null)
                return;

            if (typeof(UnityEngine.Behaviour).IsAssignableFrom(type))
                throw new System.ArgumentException($"Load asset type is invalid: '{type.FullName}'.", nameof(type));

            if (typeof(UnityEngine.Object).IsAssignableFrom(type) == false)
                throw new System.ArgumentException($"Load asset type is invalid: '{type.FullName}'.", nameof(type));
        }
        #endregion

        #region 调试信息
        internal DiagnosticPackageData GetDiagnosticData()
        {
            DiagnosticPackageData data = new DiagnosticPackageData();
            data.PackageName = PackageName;
            data.ProviderInfos = _resourceManager.DebugGetProviderInfos();
            data.BundleInfos = _resourceManager.DebugGetBundleInfos();
            data.OperationInfos = AsyncOperationSystem.GetDiagnosticInfos(PackageName);
            return data;
        }
        #endregion
    }
}