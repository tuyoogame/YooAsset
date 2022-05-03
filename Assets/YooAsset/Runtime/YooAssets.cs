using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public static class YooAssets
	{
		/// <summary>
		/// 运行模式
		/// </summary>
		public enum EPlayMode
		{
			/// <summary>
			/// 编辑器下模拟运行模式
			/// </summary>
			EditorPlayMode,

			/// <summary>
			/// 离线模式
			/// </summary>
			OfflinePlayMode,

			/// <summary>
			/// 网络模式
			/// </summary>
			HostPlayMode,
		}

		/// <summary>
		/// 初始化参数
		/// </summary>
		public abstract class CreateParameters
		{
			/// <summary>
			/// 资源定位服务接口
			/// </summary>
			public ILocationServices LocationServices = null;

			/// <summary>
			/// 文件解密服务接口
			/// </summary>
			public IDecryptionServices DecryptionServices = null;

			/// <summary>
			/// 资源加载的最大数量
			/// </summary>
			public int AssetLoadingMaxNumber = int.MaxValue;

			/// <summary>
			/// 异步操作系统每帧允许运行的最大时间切片（单位：毫秒）
			/// </summary>
			public long OperationSystemMaxTimeSlice = long.MaxValue;
		}

		/// <summary>
		/// 编辑器下模拟运行模式参数
		/// </summary>
		public class EditorPlayModeParameters : CreateParameters
		{
		}

		/// <summary>
		/// 离线模式参数
		/// </summary>
		public class OfflinePlayModeParameters : CreateParameters
		{
		}

		/// <summary>
		/// 网络模式参数
		/// </summary>
		public class HostPlayModeParameters : CreateParameters
		{
			/// <summary>
			/// 当缓存池被污染的时候清理缓存池
			/// </summary>
			public bool ClearCacheWhenDirty;

			/// <summary>
			/// 默认的资源服务器下载地址
			/// </summary>
			public string DefaultHostServer;

			/// <summary>
			/// 备用的资源服务器下载地址
			/// </summary>
			public string FallbackHostServer;

			/// <summary>
			/// 启用断点续传功能的文件大小
			/// </summary>
			public int BreakpointResumeFileSize = int.MaxValue;
		}


		private static bool _isInitialize = false;
		private static EPlayMode _playMode;
		private static IBundleServices _bundleServices;
		private static ILocationServices _locationServices;
		private static EditorPlayModeImpl _editorPlayModeImpl;
		private static OfflinePlayModeImpl _offlinePlayModeImpl;
		private static HostPlayModeImpl _hostPlayModeImpl;


		/// <summary>
		/// 异步初始化
		/// </summary>
		public static InitializationOperation InitializeAsync(CreateParameters parameters)
		{
			if (parameters == null)
				throw new Exception($"YooAsset create parameters is null.");

			if (parameters.LocationServices == null)
				throw new Exception($"{nameof(IBundleServices)} is null.");
			else
				_locationServices = parameters.LocationServices;

#if !UNITY_EDITOR
			if (parameters is EditorPlayModeParameters)
				throw new Exception($"Editor play mode only support unity editor.");
#endif

			// 创建驱动器
			if (_isInitialize == false)
			{
				_isInitialize = true;
				UnityEngine.GameObject driverGo = new UnityEngine.GameObject("[YooAsset]");
				driverGo.AddComponent<YooAssetDriver>();
				UnityEngine.Object.DontDestroyOnLoad(driverGo);
			}
			else
			{
				throw new Exception("YooAsset is initialized yet.");
			}

			if (parameters.AssetLoadingMaxNumber < 1)
			{
				parameters.AssetLoadingMaxNumber = 1;
				YooLogger.Warning($"{nameof(parameters.AssetLoadingMaxNumber)} minimum value is 1");
			}

			if (parameters.OperationSystemMaxTimeSlice < 33)
			{
				parameters.OperationSystemMaxTimeSlice = 33;
				YooLogger.Warning($"{nameof(parameters.OperationSystemMaxTimeSlice)} minimum value is 33 milliseconds");
			}

			// 运行模式
			if (parameters is EditorPlayModeParameters)
				_playMode = EPlayMode.EditorPlayMode;
			else if (parameters is OfflinePlayModeParameters)
				_playMode = EPlayMode.OfflinePlayMode;
			else if (parameters is HostPlayModeParameters)
				_playMode = EPlayMode.HostPlayMode;
			else
				throw new NotImplementedException();

			// 初始化异步操作系统
			OperationSystem.Initialize(parameters.OperationSystemMaxTimeSlice);

			// 初始化下载系统
			if (_playMode == EPlayMode.HostPlayMode)
			{
#if UNITY_WEBGL
				throw new Exception($"{EPlayMode.HostPlayMode} not supports WebGL platform !");
#else
				var hostPlayModeParameters = parameters as HostPlayModeParameters;
				DownloadSystem.Initialize(hostPlayModeParameters.BreakpointResumeFileSize);
#endif
			}

			// 初始化资源系统
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				_editorPlayModeImpl = new EditorPlayModeImpl();
				_bundleServices = _editorPlayModeImpl;
				AssetSystem.Initialize(true, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				return _editorPlayModeImpl.InitializeAsync();
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				_offlinePlayModeImpl = new OfflinePlayModeImpl();
				_bundleServices = _offlinePlayModeImpl;
				AssetSystem.Initialize(false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				return _offlinePlayModeImpl.InitializeAsync();
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				_hostPlayModeImpl = new HostPlayModeImpl();
				_bundleServices = _hostPlayModeImpl;
				AssetSystem.Initialize(false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				var hostPlayModeParameters = parameters as HostPlayModeParameters;
				return _hostPlayModeImpl.InitializeAsync(
					hostPlayModeParameters.ClearCacheWhenDirty,
					hostPlayModeParameters.DefaultHostServer,
					hostPlayModeParameters.FallbackHostServer);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 向网络端请求静态资源版本
		/// </summary>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public static UpdateStaticVersionOperation UpdateStaticVersionAsync(int timeout = 60)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				var operation = new EditorPlayModeUpdateStaticVersionOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdateStaticVersionOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.UpdateStaticVersionAsync(timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 向网络端请求并更新补丁清单
		/// </summary>
		/// <param name="resourceVersion">更新的资源版本</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public static UpdateManifestOperation UpdateManifestAsync(int resourceVersion, int timeout = 60)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				var operation = new EditorPlayModeUpdateManifestOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdateManifestOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.UpdatePatchManifestAsync(resourceVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public static int GetResourceVersion()
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				if (_editorPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _editorPlayModeImpl.GetResourceVersion();
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				if (_offlinePlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _offlinePlayModeImpl.GetResourceVersion();
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.GetResourceVersion();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public static void UnloadUnusedAssets()
		{
			AssetSystem.Update();
			AssetSystem.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public static void ForceUnloadAllAssets()
		{
			AssetSystem.ForceUnloadAllAssets();
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static BundleInfo GetBundleInfo(string location)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			string bundleName = _bundleServices.GetBundleName(assetPath);
			return _bundleServices.GetBundleInfo(bundleName);
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static BundleInfo GetBundleInfo(AssetInfo assetInfo)
		{
			string bundleName = _bundleServices.GetBundleName(assetInfo.AssetPath);
			return _bundleServices.GetBundleInfo(bundleName);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <returns></returns>
		public static AssetInfo[] GetAssetInfos(string tag)
		{
			string[] tags = new string[] { tag };
			return _bundleServices.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取调试信息
		/// </summary>
		internal static void GetDebugReport(DebugReport report)
		{
			if (report == null)
				YooLogger.Error($"{nameof(DebugReport)} is null");

			AssetSystem.GetDebugReport(report);
		}

		#region 场景加载
		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			string scenePath = _locationServices.ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadSceneAsync(scenePath, sceneMode, activateOnLoad, priority);
			return handle;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			string scenePath = assetInfo.AssetPath;
			var handle = AssetSystem.LoadSceneAsync(scenePath, sceneMode, activateOnLoad, priority);
			return handle;
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 异步获取原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="copyPath">拷贝路径</param>
		public static RawFileOperation GetRawFileAsync(string location, string copyPath = null)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return GetRawFileInternal(assetPath, copyPath);
		}

		/// <summary>
		/// 异步获取原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		/// <param name="copyPath">拷贝路径</param>
		public static RawFileOperation GetRawFileAsync(AssetInfo assetInfo, string copyPath = null)
		{
			return GetRawFileInternal(assetInfo.AssetPath, copyPath);
		}


		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			return LoadAssetInternal(assetInfo.AssetPath, assetInfo.AssetType, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : class
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadAssetInternal(assetPath, typeof(TObject), true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public static AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadAssetInternal(assetPath, type, true);
		}


		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			return LoadSubAssetsInternal(assetInfo.AssetPath, assetInfo.AssetType, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadSubAssetsInternal(assetPath, typeof(TObject), true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadSubAssetsInternal(assetPath, type, true);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			return LoadAssetInternal(assetInfo.AssetPath, assetInfo.AssetType, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetAsync<TObject>(string location)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadAssetInternal(assetPath, typeof(TObject), false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public static AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadAssetInternal(assetPath, type, false);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			return LoadSubAssetsInternal(assetInfo.AssetPath, assetInfo.AssetType, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadSubAssetsInternal(assetPath, typeof(TObject), false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			return LoadSubAssetsInternal(assetPath, type, false);
		}


		private static RawFileOperation GetRawFileInternal(string assetPath, string copyPath)
		{
			string bundleName = _bundleServices.GetBundleName(assetPath);
			BundleInfo bundleInfo = _bundleServices.GetBundleInfo(bundleName);

			if (_playMode == EPlayMode.EditorPlayMode)
			{
				RawFileOperation operation = new EditorPlayModeRawFileOperation(bundleInfo, copyPath);
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				RawFileOperation operation = new OfflinePlayModeRawFileOperation(bundleInfo, copyPath);
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				RawFileOperation operation = new HostPlayModeRawFileOperation(bundleInfo, copyPath);
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		private static AssetOperationHandle LoadAssetInternal(string assetPath, System.Type assetType, bool waitForAsyncComplete)
		{
			var handle = AssetSystem.LoadAssetAsync(assetPath, assetType);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		private static SubAssetsOperationHandle LoadSubAssetsInternal(string assetPath, System.Type assetType, bool waitForAsyncComplete)
		{
			var handle = AssetSystem.LoadSubAssetsAsync(assetPath, assetType);
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
		public static PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
		{
			return CreatePatchDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.CreatePatchDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain);
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
		public static PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.CreatePatchDownloaderByAll(downloadingMaxNumber, failedTryAgain);
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
		public static PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");

				List<string> assetPaths = new List<string>(locations.Length);
				foreach (var location in locations)
				{
					string assetPath = _locationServices.ConvertLocationToAssetPath(location);
					assetPaths.Add(assetPath);
				}
				return _hostPlayModeImpl.CreatePatchDownloaderByPaths(assetPaths, downloadingMaxNumber, failedTryAgain);
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
		public static PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");

				List<string> assetPaths = new List<string>(assetInfos.Length);
				foreach (var assetInfo in assetInfos)
				{
					assetPaths.Add(assetInfo.AssetPath);
				}
				return _hostPlayModeImpl.CreatePatchDownloaderByPaths(assetPaths, downloadingMaxNumber, failedTryAgain);
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
		public static PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			return CreatePatchUnpacker(new string[] { tag }, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				if (_offlinePlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _offlinePlayModeImpl.CreatePatchUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain);
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.CreatePatchUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain);
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
		/// <param name="resourceVersion">指定更新的资源版本</param>
		/// <param name="timeout">超时时间</param>
		public static UpdatePackageOperation UpdatePackageAsync(int resourceVersion, int timeout = 60)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				var operation = new EditorPlayModeUpdatePackageOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdatePackageOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.UpdatePackageAsync(resourceVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 沙盒相关
		/// <summary>
		/// 获取沙盒的根路径
		/// </summary>
		public static string GetSandboxRoot()
		{
			return PathHelper.MakePersistentRootPath();
		}

		/// <summary>
		/// 清空沙盒目录
		/// </summary>
		public static void ClearSandbox()
		{
			SandboxHelper.DeleteSandbox();
		}

		/// <summary>
		/// 清空所有的缓存文件
		/// </summary>
		public static void ClearAllCacheFiles()
		{
			SandboxHelper.DeleteCacheFolder();
		}

		/// <summary>
		/// 清空未被使用的缓存文件
		/// </summary>
		public static void ClearUnusedCacheFiles()
		{
			if (_playMode == EPlayMode.HostPlayMode)
				_hostPlayModeImpl.ClearUnusedCacheFiles();
		}
		#endregion

		#region 内部方法
		internal static void InternalUpdate()
		{
			// 更新异步请求操作
			OperationSystem.Update();

			// 更新下载管理系统
			DownloadSystem.Update();

			// 轮询更新资源系统
			AssetSystem.Update();
		}
		internal static string MappingToAssetPath(string location)
		{
#if UNITY_EDITOR
			CheckLocation(location);
#endif

			return _bundleServices.MappingToAssetPath(location);
		}

#if UNITY_EDITOR
		private static void CheckLocation(string location)
		{
			if (string.IsNullOrEmpty(location))
			{
				UnityEngine.Debug.LogError("location param is null or empty!");
			}
			else
			{
				// 检查路径末尾是否有空格
				int index = location.LastIndexOf(" ");
				if (index != -1)
				{
					if (location.Length == index + 1)
						UnityEngine.Debug.LogWarning($"Found blank character in location : \"{location}\"");
				}

				if (location.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
					UnityEngine.Debug.LogWarning($"Found illegal character in location : \"{location}\"");
			}
		}
#endif
		#endregion
	}
}