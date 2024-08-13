using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    public class ResourcePackage
    {
        private bool _isInitialize = false;
        private string _initializeError = string.Empty;
        private EOperationStatus _initializeStatus = EOperationStatus.None;
        private EPlayMode _playMode;

        // 管理器
        private ResourceManager _resourceManager;
        private IBundleQuery _bundleQuery;
        private IPlayMode _playModeImpl;

        /// <summary>
        /// 包裹名
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 初始化状态
        /// </summary>
        public EOperationStatus InitializeStatus
        {
            get { return _initializeStatus; }
        }


        internal ResourcePackage(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 更新资源包裹
        /// </summary>
        internal void UpdatePackage()
        {
            if (_playModeImpl != null)
                _playModeImpl.UpdatePlayMode();
        }

        /// <summary>
        /// 销毁资源包裹
        /// </summary>
        internal void DestroyPackage()
        {
            if (_isInitialize)
            {
                _isInitialize = false;
                _initializeError = string.Empty;
                _initializeStatus = EOperationStatus.None;

                _resourceManager = null;
                _bundleQuery = null;
                _playModeImpl = null;
            }
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            // 注意：联机平台因为网络原因可能会初始化失败！
            ResetInitializeAfterFailed();

            // 检测初始化参数合法性
            CheckInitializeParameters(parameters);

            // 创建资源管理器
            InitializationOperation initializeOperation;
            _resourceManager = new ResourceManager(PackageName);
            if (_playMode == EPlayMode.EditorSimulateMode)
            {
                var editorSimulateModeImpl = new EditorSimulateModeImpl(PackageName);
                _bundleQuery = editorSimulateModeImpl;
                _playModeImpl = editorSimulateModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as EditorSimulateModeParameters;
                initializeOperation = editorSimulateModeImpl.InitializeAsync(initializeParameters);
            }
            else if (_playMode == EPlayMode.OfflinePlayMode)
            {
                var offlinePlayModeImpl = new OfflinePlayModeImpl(PackageName);
                _bundleQuery = offlinePlayModeImpl;
                _playModeImpl = offlinePlayModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as OfflinePlayModeParameters;
                initializeOperation = offlinePlayModeImpl.InitializeAsync(initializeParameters);
            }
            else if (_playMode == EPlayMode.HostPlayMode)
            {
                var hostPlayModeImpl = new HostPlayModeImpl(PackageName);
                _bundleQuery = hostPlayModeImpl;
                _playModeImpl = hostPlayModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as HostPlayModeParameters;
                initializeOperation = hostPlayModeImpl.InitializeAsync(initializeParameters);
            }
            else if (_playMode == EPlayMode.WebPlayMode)
            {
                var webPlayModeImpl = new WebPlayModeImpl(PackageName);
                _bundleQuery = webPlayModeImpl;
                _playModeImpl = webPlayModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as WebPlayModeParameters;
                initializeOperation = webPlayModeImpl.InitializeAsync(initializeParameters);
            }
            else
            {
                throw new NotImplementedException();
            }

            // 监听初始化结果
            _isInitialize = true;
            initializeOperation.Completed += InitializeOperation_Completed;
            return initializeOperation;
        }
        private void ResetInitializeAfterFailed()
        {
            if (_isInitialize && _initializeStatus == EOperationStatus.Failed)
            {
                _isInitialize = false;
                _initializeStatus = EOperationStatus.None;
                _initializeError = string.Empty;
            }
        }
        private void CheckInitializeParameters(InitializeParameters parameters)
        {
            if (_isInitialize)
                throw new Exception($"{nameof(ResourcePackage)} is initialized yet.");

            if (parameters == null)
                throw new Exception($"{nameof(ResourcePackage)} create parameters is null.");

#if !UNITY_EDITOR
            if (parameters is EditorSimulateModeParameters)
                throw new Exception($"Editor simulate mode only support unity editor.");
#endif

            // 鉴定运行模式
            if (parameters is EditorSimulateModeParameters)
                _playMode = EPlayMode.EditorSimulateMode;
            else if (parameters is OfflinePlayModeParameters)
                _playMode = EPlayMode.OfflinePlayMode;
            else if (parameters is HostPlayModeParameters)
                _playMode = EPlayMode.HostPlayMode;
            else if (parameters is WebPlayModeParameters)
                _playMode = EPlayMode.WebPlayMode;
            else
                throw new NotImplementedException();

            // 检测运行时平台
            if (_playMode != EPlayMode.EditorSimulateMode)
            {
#if UNITY_WEBGL
                if (_playMode != EPlayMode.WebPlayMode)
                {
                    throw new Exception($"{_playMode} can not support WebGL plateform !");
                }
#else
                if (_playMode == EPlayMode.WebPlayMode)
                {
                    throw new Exception($"{nameof(EPlayMode.WebPlayMode)} only support WebGL plateform !");
                }
#endif
            }
        }
        private void InitializeOperation_Completed(AsyncOperationBase op)
        {
            _initializeStatus = op.Status;
            _initializeError = op.Error;
        }

        /// <summary>
        /// 异步销毁
        /// </summary>
        public DestroyOperation DestroyAsync()
        {
            var operation = new DestroyOperation(this);
            OperationSystem.StartOperation(null, operation);
            return operation;
        }

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        /// <param name="appendTimeTicks">在URL末尾添加时间戳</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        public RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
        {
            DebugCheckInitialize(false);
            return _playModeImpl.RequestPackageVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        /// <param name="packageVersion">更新的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize(false);

            // 注意：强烈建议在更新之前保持加载器为空！
            if (_resourceManager.HasAnyLoader())
            {
                YooLogger.Warning($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(UnloadAllAssetsAsync)} method to release loaded bundle !");
            }

            return _playModeImpl.UpdatePackageManifestAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 预下载指定版本的包裹资源
        /// </summary>
        /// <param name="packageVersion">下载的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        public PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize(false);
            return _playModeImpl.PreDownloadContentAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 清理文件系统所有的资源文件
        /// </summary>
        public ClearAllBundleFilesOperation ClearAllBundleFilesAsync()
        {
            DebugCheckInitialize();
            return _playModeImpl.ClearAllBundleFilesAsync();
        }

        /// <summary>
        /// 清理文件系统未使用的资源文件
        /// </summary>
        public ClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync()
        {
            DebugCheckInitialize();
            return _playModeImpl.ClearUnusedBundleFilesAsync();
        }

        /// <summary>
        /// 获取本地包裹的版本信息
        /// </summary>
        public string GetPackageVersion()
        {
            DebugCheckInitialize();
            return _playModeImpl.ActiveManifest.PackageVersion;
        }

        #region 资源回收
        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        public UnloadAllAssetsOperation UnloadAllAssetsAsync()
        {
            DebugCheckInitialize();
            var operation = new UnloadAllAssetsOperation(_resourceManager);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 回收不再使用的资源
        /// 说明：卸载引用计数为零的资源
        /// </summary>
        public UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync()
        {
            DebugCheckInitialize();
            var operation = new UnloadUnusedAssetsOperation(_resourceManager);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 资源回收
        /// 说明：尝试卸载指定的资源
        /// </summary>
        public void TryUnloadUnusedAsset(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            _resourceManager.TryUnloadUnusedAsset(assetInfo);
        }

        /// <summary>
        /// 资源回收
        /// 说明：尝试卸载指定的资源
        /// </summary>
        public void TryUnloadUnusedAsset(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            _resourceManager.TryUnloadUnusedAsset(assetInfo);
        }
        #endregion

        #region 资源信息
        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public bool IsNeedDownloadFromRemote(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return IsNeedDownloadFromRemoteInternal(assetInfo);
        }

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return IsNeedDownloadFromRemoteInternal(assetInfo);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tag">资源标签</param>
        public AssetInfo[] GetAssetInfos(string tag)
        {
            DebugCheckInitialize();
            string[] tags = new string[] { tag };
            return _playModeImpl.ActiveManifest.GetAssetsInfoByTags(tags);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        public AssetInfo[] GetAssetInfos(string[] tags)
        {
            DebugCheckInitialize();
            return _playModeImpl.ActiveManifest.GetAssetsInfoByTags(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public AssetInfo GetAssetInfo(string location)
        {
            DebugCheckInitialize();
            return ConvertLocationToAssetInfo(location, null);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetInfo GetAssetInfo(string location, System.Type type)
        {
            DebugCheckInitialize();
            return ConvertLocationToAssetInfo(location, type);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        public AssetInfo GetAssetInfoByGUID(string assetGUID)
        {
            DebugCheckInitialize();
            return ConvertAssetGUIDToAssetInfo(assetGUID, null);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        /// <param name="type">资源类型</param>
        public AssetInfo GetAssetInfoByGUID(string assetGUID, System.Type type)
        {
            DebugCheckInitialize();
            return ConvertAssetGUIDToAssetInfo(assetGUID, type);
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public bool CheckLocationValid(string location)
        {
            DebugCheckInitialize();
            string assetPath = _playModeImpl.ActiveManifest.TryMappingToAssetPath(location);
            return string.IsNullOrEmpty(assetPath) == false;
        }

        private bool IsNeedDownloadFromRemoteInternal(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Warning(assetInfo.Error);
                return false;
            }

            BundleInfo bundleInfo = _bundleQuery.GetMainBundleInfo(assetInfo);
            if (bundleInfo.IsNeedDownloadFromRemote())
                return true;

            BundleInfo[] depends = _bundleQuery.GetDependBundleInfos(assetInfo);
            foreach (var depend in depends)
            {
                if (depend.IsNeedDownloadFromRemote())
                    return true;
            }

            return false;
        }
        #endregion

        #region 原生文件
        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadRawFileInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public RawFileHandle LoadRawFileSync(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        public RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadRawFileInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public RawFileHandle LoadRawFileAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, false, priority);
        }


        private RawFileHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            var handle = _resourceManager.LoadRawFileAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 场景加载
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        public SceneHandle LoadSceneSync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadSceneInternal(assetInfo, true, sceneMode, physicsMode, false, 0);
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        public SceneHandle LoadSceneSync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            DebugCheckInitialize();
            return LoadSceneInternal(assetInfo, true, sceneMode, physicsMode, false, 0);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">加载的优先级</param>
        public SceneHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool suspendLoad = false, uint priority = 0)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadSceneInternal(assetInfo, false, sceneMode, physicsMode, suspendLoad, priority);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">加载的优先级</param>
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool suspendLoad = false, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadSceneInternal(assetInfo, false, sceneMode, physicsMode, suspendLoad, priority);
        }

        private SceneHandle LoadSceneInternal(AssetInfo assetInfo, bool waitForAsyncComplete, LoadSceneMode sceneMode, LocalPhysicsMode physicsMode, bool suspendLoad, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var loadSceneParams = new LoadSceneParameters(sceneMode, physicsMode);
            var handle = _resourceManager.LoadSceneAsync(assetInfo, loadSceneParams, suspendLoad, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public AssetHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetHandle LoadAssetSync(string location, System.Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public AssetHandle LoadAssetSync(string location)
        {
            DebugCheckInitialize();
            Type type = typeof(UnityEngine.Object);
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        /// <param name="priority">加载的优先级</param>
        public AssetHandle LoadAssetAsync(string location, System.Type type, uint priority = 0)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public AssetHandle LoadAssetAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            Type type = typeof(UnityEngine.Object);
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, false, priority);
        }


        private AssetHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var handle = _resourceManager.LoadAssetAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public SubAssetsHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public SubAssetsHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public SubAssetsHandle LoadSubAssetsSync(string location, System.Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public SubAssetsHandle LoadSubAssetsSync(string location)
        {
            DebugCheckInitialize();
            Type type = typeof(UnityEngine.Object);
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public SubAssetsHandle LoadSubAssetsAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <param name="priority">加载的优先级</param>
        public SubAssetsHandle LoadSubAssetsAsync(string location, System.Type type, uint priority = 0)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public SubAssetsHandle LoadSubAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            Type type = typeof(UnityEngine.Object);
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }


        private SubAssetsHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var handle = _resourceManager.LoadSubAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public AllAssetsHandle LoadAllAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public AllAssetsHandle LoadAllAssetsSync(string location, System.Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public AllAssetsHandle LoadAllAssetsSync(string location)
        {
            DebugCheckInitialize();
            Type type = typeof(UnityEngine.Object);
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public AllAssetsHandle LoadAllAssetsAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <param name="priority">加载的优先级</param>
        public AllAssetsHandle LoadAllAssetsAsync(string location, System.Type type, uint priority = 0)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        public AllAssetsHandle LoadAllAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            Type type = typeof(UnityEngine.Object);
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }


        private AllAssetsHandle LoadAllAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var handle = _resourceManager.LoadAllAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }
        #endregion

        #region 资源下载
        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByAll(downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByTags(new string[] { tag }, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源的定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
            foreach (var location in locations)
            {
                var assetInfo = ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos.ToArray(), downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }
        #endregion

        #region 资源解压
        /// <summary>
        /// 创建内置资源解压器，用于解压当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceUnpackerByAll(unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceUnpackerByTags(new string[] { tag }, unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }
        #endregion

        #region 资源导入
        /// <summary>
        /// 创建资源导入器
        /// 注意：资源文件名称必须和资源服务器部署的文件名称一致！
        /// </summary>
        /// <param name="filePaths">资源路径列表</param>
        /// <param name="importerMaxNumber">同时导入的最大文件数</param>
        /// <param name="failedTryAgain">导入失败的重试次数</param>
        public ResourceImporterOperation CreateResourceImporter(string[] filePaths, int importerMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceImporterByFilePaths(filePaths, importerMaxNumber, failedTryAgain, int.MaxValue);
        }
        #endregion

        #region 内部方法
        private AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
        {
            return _playModeImpl.ActiveManifest.ConvertLocationToAssetInfo(location, assetType);
        }
        private AssetInfo ConvertAssetGUIDToAssetInfo(string assetGUID, System.Type assetType)
        {
            return _playModeImpl.ActiveManifest.ConvertAssetGUIDToAssetInfo(assetGUID, assetType);
        }
        #endregion

        #region 调试方法
        [Conditional("DEBUG")]
        private void DebugCheckInitialize(bool checkActiveManifest = true)
        {
            if (_initializeStatus == EOperationStatus.None)
                throw new Exception("Package initialize not completed !");
            else if (_initializeStatus == EOperationStatus.Failed)
                throw new Exception($"Package initialize failed ! {_initializeError}");

            if (checkActiveManifest)
            {
                if (_playModeImpl.ActiveManifest == null)
                    throw new Exception("Can not found active package manifest !");
            }
        }

        [Conditional("DEBUG")]
        private void DebugCheckAssetLoadType(System.Type type)
        {
            if (type == null)
                return;

            if (typeof(UnityEngine.Behaviour).IsAssignableFrom(type))
            {
                throw new Exception($"Load asset type is invalid : {type.FullName} !");
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type) == false)
            {
                throw new Exception($"Load asset type is invalid : {type.FullName} !");
            }
        }
        #endregion

        #region 调试信息
        internal DebugPackageData GetDebugPackageData()
        {
            DebugPackageData data = new DebugPackageData();
            data.PackageName = PackageName;
            data.ProviderInfos = _resourceManager.GetDebugReportInfos();
            return data;
        }
        #endregion
    }
}