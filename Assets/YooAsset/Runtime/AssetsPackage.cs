using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public class AssetsPackage
	{
		private bool _isInitialize = false;
		private string _initializeError = string.Empty;
		private EOperationStatus _initializeStatus = EOperationStatus.None;
		private EPlayMode _playMode;
		private IBundleServices _bundleServices;
		private AssetSystemImpl _assetSystemImpl;
		private EditorSimulateModeImpl _editorSimulateModeImpl;
		private OfflinePlayModeImpl _offlinePlayModeImpl;
		private HostPlayModeImpl _hostPlayModeImpl;

		/// <summary>
		/// 包裹名
		/// </summary>
		public string PackageName { private set; get; }

		/// <summary>
		/// 初始化状态
		/// </summary>
		public EOperationStatus InitializeStatus
		{
			get { return _initializeStatus; }
		}


		private AssetsPackage()
		{
		}
		internal AssetsPackage(string packageName)
		{
			PackageName = packageName;
		}

		/// <summary>
		/// 更新资源包裹
		/// </summary>
		internal void UpdatePackage()
		{
			if (_assetSystemImpl != null)
				_assetSystemImpl.Update();
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

				_bundleServices = null;
				_editorSimulateModeImpl = null;
				_offlinePlayModeImpl = null;
				_hostPlayModeImpl = null;

				if (_assetSystemImpl != null)
				{
					_assetSystemImpl.DestroyAll();
					_assetSystemImpl = null;
				}
			}
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(InitializeParameters parameters)
		{
			// 检测初始化参数合法性
			CheckInitializeParameters(parameters);

			// 初始化资源系统
			InitializationOperation initializeOperation;
			_assetSystemImpl = new AssetSystemImpl();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				_editorSimulateModeImpl = new EditorSimulateModeImpl();
				_bundleServices = _editorSimulateModeImpl;
				_assetSystemImpl.Initialize(true, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				var initializeParameters = parameters as EditorSimulateModeParameters;
				initializeOperation = _editorSimulateModeImpl.InitializeAsync(
					initializeParameters.LocationToLower,
					initializeParameters.SimulatePatchManifestPath);
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				_offlinePlayModeImpl = new OfflinePlayModeImpl();
				_bundleServices = _offlinePlayModeImpl;
				_assetSystemImpl.Initialize(false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				var initializeParameters = parameters as OfflinePlayModeParameters;
				initializeOperation = _offlinePlayModeImpl.InitializeAsync(
					initializeParameters.LocationToLower,
					PackageName);
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				_hostPlayModeImpl = new HostPlayModeImpl();
				_bundleServices = _hostPlayModeImpl;
				_assetSystemImpl.Initialize(false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				var initializeParameters = parameters as HostPlayModeParameters;
				initializeOperation = _hostPlayModeImpl.InitializeAsync(
					initializeParameters.LocationToLower,
					initializeParameters.DefaultHostServer,
					initializeParameters.FallbackHostServer,
					initializeParameters.QueryServices,
					PackageName);
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
		private void CheckInitializeParameters(InitializeParameters parameters)
		{
			if (_isInitialize)
				throw new Exception($"{nameof(AssetsPackage)} is initialized yet.");

			if (parameters == null)
				throw new Exception($"{nameof(AssetsPackage)} create parameters is null.");

#if !UNITY_EDITOR
			if (parameters is EditorSimulateModeParameters)
				throw new Exception($"Editor simulate mode only support unity editor.");
#endif

			if (parameters is EditorSimulateModeParameters)
			{
				var editorSimulateModeParameters = parameters as EditorSimulateModeParameters;
				if (string.IsNullOrEmpty(editorSimulateModeParameters.SimulatePatchManifestPath))
					throw new Exception($"{nameof(editorSimulateModeParameters.SimulatePatchManifestPath)} is null or empty.");
			}

			if (parameters is HostPlayModeParameters)
			{
				var hostPlayModeParameters = parameters as HostPlayModeParameters;
				if (string.IsNullOrEmpty(hostPlayModeParameters.DefaultHostServer))
					throw new Exception($"${hostPlayModeParameters.DefaultHostServer} is null or empty.");
				if (string.IsNullOrEmpty(hostPlayModeParameters.FallbackHostServer))
					throw new Exception($"${hostPlayModeParameters.FallbackHostServer} is null or empty.");
				if (hostPlayModeParameters.QueryServices == null)
					throw new Exception($"{nameof(IQueryServices)} is null.");
			}

			// 鉴定运行模式
			if (parameters is EditorSimulateModeParameters)
				_playMode = EPlayMode.EditorSimulateMode;
			else if (parameters is OfflinePlayModeParameters)
				_playMode = EPlayMode.OfflinePlayMode;
			else if (parameters is HostPlayModeParameters)
				_playMode = EPlayMode.HostPlayMode;
			else
				throw new NotImplementedException();

			// 检测参数范围
			if (parameters.AssetLoadingMaxNumber < 1)
			{
				parameters.AssetLoadingMaxNumber = 1;
				YooLogger.Warning($"{nameof(parameters.AssetLoadingMaxNumber)} minimum value is 1");
			}
		}
		private void InitializeOperation_Completed(AsyncOperationBase op)
		{
			_initializeStatus = op.Status;
			_initializeError = op.Error;
		}

		/// <summary>
		/// 向网络端请求最新的资源版本
		/// </summary>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public UpdatePackageVersionOperation UpdatePackageVersionAsync(int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorPlayModeUpdatePackageVersionOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdatePackageVersionOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.UpdatePackageVersionAsync(PackageName, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 向网络端请求并更新补丁清单
		/// </summary>
		/// <param name="packageVersion">更新的包裹版本</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
		{
			DebugCheckInitialize();
			DebugCheckUpdateManifest();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorPlayModeUpdatePackageManifestOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdatePackageManifestOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.UpdatePackageManifestAsync(PackageName, packageVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 检查本地包裹内容的完整性
		/// </summary>
		public CheckPackageContentsOperation CheckPackageContentsAsync()
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorSimulateModeCheckPackageContentsOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeCheckPackageContentsOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CheckPackageContentsAsync(PackageName);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 清理本地包裹未使用的缓存文件
		/// </summary>
		public ClearPackageUnusedCacheFilesOperation ClearPackageUnusedCacheFilesAsync()
		{
			DebugCheckInitialize();
			var operation = new ClearPackageUnusedCacheFilesOperation(this);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 获取本地包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				return _editorSimulateModeImpl.GetPackageVersion();
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				return _offlinePlayModeImpl.GetPackageVersion();
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.GetPackageVersion();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public void UnloadUnusedAssets()
		{
			DebugCheckInitialize();
			_assetSystemImpl.Update();
			_assetSystemImpl.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceUnloadAllAssets()
		{
			DebugCheckInitialize();
			_assetSystemImpl.ForceUnloadAllAssets();
		}


		#region 资源信息
		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool IsNeedDownloadFromRemote(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				return false;
			}

			BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
			if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				return false;
			}

			BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
			if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tag">资源标签</param>
		public AssetInfo[] GetAssetInfos(string tag)
		{
			DebugCheckInitialize();
			string[] tags = new string[] { tag };
			return _bundleServices.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		public AssetInfo[] GetAssetInfos(string[] tags)
		{
			DebugCheckInitialize();
			return _bundleServices.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取资源信息
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public AssetInfo GetAssetInfo(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return assetInfo;
		}

		/// <summary>
		/// 检查资源定位地址是否有效
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool CheckLocationValid(string location)
		{
			DebugCheckInitialize();
			string assetPath = _bundleServices.TryMappingToAssetPath(location);
			return string.IsNullOrEmpty(assetPath) == false;
		}
		#endregion

		#region 原生文件
		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadRawFileInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public RawFileOperationHandle LoadRawFileSync(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return LoadRawFileInternal(assetInfo, true);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadRawFileInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public RawFileOperationHandle LoadRawFileAsync(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return LoadRawFileInternal(assetInfo, false);
		}


		private RawFileOperationHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile == false)
					throw new Exception($"Cannot load asset bundle file using {nameof(LoadRawFileAsync)} method !");
			}
#endif

			var handle = _assetSystemImpl.LoadRawFileAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 场景加载
		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			var handle = _assetSystemImpl.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			var handle = _assetSystemImpl.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, false);
		}


		private AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile)
					throw new Exception($"Cannot load raw file using {nameof(LoadAssetAsync)} method !");
			}
#endif

			var handle = _assetSystemImpl.LoadAssetAsync(assetInfo);
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
		public SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, false);
		}


		private SubAssetsOperationHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile)
					throw new Exception($"Cannot load raw file using {nameof(LoadSubAssetsAsync)} method !");
			}
#endif

			var handle = _assetSystemImpl.LoadSubAssetsAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 资源下载
		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return CreatePatchDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchDownloaderByAll(downloadingMaxNumber, failedTryAgain, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}


		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源定位列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
				foreach (var location in locations)
				{
					AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
					assetInfos.Add(assetInfo);
				}
				return _hostPlayModeImpl.CreatePatchDownloaderByPaths(assetInfos.ToArray(), downloadingMaxNumber, failedTryAgain, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 资源解压
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return CreatePatchUnpacker(new string[] { tag }, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain, int.MaxValue);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain, int.MaxValue);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public PatchUnpackerOperation CreatePatchUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain, int.MaxValue);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain, int.MaxValue);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchUnpackerByAll(unpackingMaxNumber, failedTryAgain, int.MaxValue);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 包裹更新
		/// <summary>
		/// 创建资源包裹下载器，用于下载更新指定资源版本所有的资源包文件
		/// </summary>
		/// <param name="packageVersion">指定更新的包裹版本</param>
		/// <param name="timeout">超时时间</param>
		public DownloadPackageOperation DownloadPackageAsync(string packageVersion, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorPlayModeDownloadPackageOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeDownloadPackageOperation();
				OperationSystem.StartOperation(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.DownloadPackageAsync(PackageName, packageVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 内部方法
		/// <summary>
		/// 是否包含资源文件
		/// </summary>
		internal bool IsIncludeBundleFile(string fileName)
		{
			// NOTE : 编辑器模拟模式下始终返回TRUE
			if (_playMode == EPlayMode.EditorSimulateMode)
				return true;
			return _bundleServices.IsIncludeBundleFile(fileName);
		}
		#endregion

		#region 调试方法
		[Conditional("DEBUG")]
		private void DebugCheckInitialize()
		{
			if (_initializeStatus == EOperationStatus.None)
				throw new Exception("Package initialize not completed !");
			else if (_initializeStatus == EOperationStatus.Failed)
				throw new Exception($"Package initialize failed ! {_initializeError}");
		}

		[Conditional("DEBUG")]
		private void DebugCheckLocation(string location)
		{
			if (string.IsNullOrEmpty(location) == false)
			{
				// 检查路径末尾是否有空格
				int index = location.LastIndexOf(" ");
				if (index != -1)
				{
					if (location.Length == index + 1)
						YooLogger.Warning($"Found blank character in location : \"{location}\"");
				}

				if (location.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
					YooLogger.Warning($"Found illegal character in location : \"{location}\"");
			}
		}

		[Conditional("DEBUG")]
		private void DebugCheckUpdateManifest()
		{
			var loadedBundleInfos = _assetSystemImpl.GetLoadedBundleInfos();
			if (loadedBundleInfos.Count > 0)
			{
				YooLogger.Error($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(ForceUnloadAllAssets)} method to release loaded bundle !");
			}
		}
		#endregion

		#region 调试信息
		internal DebugPackageData GetDebugPackageData()
		{
			DebugPackageData data = new DebugPackageData();
			data.PackageName = PackageName;
			data.ProviderInfos = _assetSystemImpl.GetDebugReportInfos();
			return data;
		}
		#endregion

		#region 私有方法
		/// <summary>
		/// 资源定位地址转换为资源信息类，失败时内部会发出错误日志。
		/// </summary>
		/// <returns>如果转换失败会返回一个无效的资源信息类</returns>
		private AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
		{
			DebugCheckLocation(location);
			string assetPath = _bundleServices.MappingToAssetPath(location);
			PatchAsset patchAsset = _bundleServices.TryGetPatchAsset(assetPath);
			if (patchAsset != null)
			{
				AssetInfo assetInfo = new AssetInfo(patchAsset, assetType);
				return assetInfo;
			}
			else
			{
				string error;
				if (string.IsNullOrEmpty(location))
					error = $"The location is null or empty !";
				else
					error = $"The location is invalid : {location}";
				AssetInfo assetInfo = new AssetInfo(error);
				return assetInfo;
			}
		}
		#endregion
	}
}