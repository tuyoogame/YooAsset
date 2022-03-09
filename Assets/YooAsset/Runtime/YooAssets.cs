using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	public static class YooAssets
	{
		/// <summary>
		/// 运行模式
		/// </summary>
		private enum EPlayMode
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

		public abstract class CreateParameters
		{
			/// <summary>
			/// 资源定位的根路径
			/// 例如：Assets/MyResource
			/// </summary>
			public string LocationRoot;

			/// <summary>
			/// 文件解密接口
			/// </summary>
			public IDecryptionServices DecryptionServices = null;

			/// <summary>
			/// 资源系统自动释放零引用资源的间隔秒数
			/// 注意：如果小于等于零代表不自动释放，可以使用YooAssets.UnloadUnusedAssets接口主动释放
			/// </summary>
			public float AutoReleaseInterval = -1;

			/// <summary>
			/// 资源加载的最大数量
			/// </summary>
			public int AssetLoadingMaxNumber = int.MaxValue;
		}

		/// <summary>
		/// 编辑器下模拟运行（只支持在编辑器下运行）
		/// </summary>
		public class EditorPlayModeParameters : CreateParameters
		{
		}

		/// <summary>
		/// 离线模式（本地打包运行模式）
		/// </summary>
		public class OfflinePlayModeParameters : CreateParameters
		{
		}

		/// <summary>
		/// 网络模式（网络打包运行模式）
		/// </summary>
		public class HostPlayModeParameters : CreateParameters
		{
			/// <summary>
			/// 当缓存池被污染的时候清理缓存池
			/// </summary>
			public bool ClearCacheWhenDirty;

			/// <summary>
			/// 忽略资源版本号
			/// </summary>
			public bool IgnoreResourceVersion;

			/// <summary>
			/// 默认的资源服务器下载地址
			/// </summary>
			public string DefaultHostServer;

			/// <summary>
			/// 备用的资源服务器下载地址
			/// </summary>
			public string FallbackHostServer;
		}


		private static bool _isInitialize = false;
		private static string _locationRoot;
		private static EPlayMode _playMode;
		private static IBundleServices _bundleServices;
		private static EditorPlayModeImpl _editorPlayModeImpl;
		private static OfflinePlayModeImpl _offlinePlayModeImpl;
		private static HostPlayModeImpl _hostPlayModeImpl;

		private static float _releaseTimer;
		private static float _releaseCD = -1f;


		/// <summary>
		/// 异步初始化
		/// </summary>
		public static InitializationOperation InitializeAsync(CreateParameters parameters)
		{
			if (parameters == null)
				throw new Exception($"YooAsset create parameters is invalid.");

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

			// 检测创建参数
			if (parameters.AssetLoadingMaxNumber < 3)
			{
				parameters.AssetLoadingMaxNumber = 3;
				YooLogger.Warning($"{nameof(parameters.AssetLoadingMaxNumber)} minimum is 3");
			}

			// 创建间隔计时器
			if (parameters.AutoReleaseInterval > 0)
			{
				_releaseCD = parameters.AutoReleaseInterval;
			}

			if (string.IsNullOrEmpty(parameters.LocationRoot) == false)
				_locationRoot = PathHelper.GetRegularPath(parameters.LocationRoot);

			// 运行模式
			if (parameters is EditorPlayModeParameters)
				_playMode = EPlayMode.EditorPlayMode;
			else if (parameters is OfflinePlayModeParameters)
				_playMode = EPlayMode.OfflinePlayMode;
			else if (parameters is HostPlayModeParameters)
				_playMode = EPlayMode.HostPlayMode;
			else
				throw new NotImplementedException();

			// 初始化
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
					hostPlayModeParameters.IgnoreResourceVersion,
					hostPlayModeParameters.DefaultHostServer,
					hostPlayModeParameters.FallbackHostServer);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 向网络端请求并更新补丁清单
		/// </summary>
		/// <param name="updateResourceVersion">更新的资源版本号</param>
		/// <param name="timeout">超时时间</param>
		public static UpdateManifestOperation UpdateManifestAsync(int updateResourceVersion, int timeout)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				var operation = new EditorModeUpdateManifestOperation();
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
				return _hostPlayModeImpl.UpdatePatchManifestAsync(updateResourceVersion, timeout);
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
		/// 获取资源包信息
		/// </summary>
		public static BundleInfo GetBundleInfo(string location)
		{
			string assetPath = ConvertLocationToAssetPath(location);
			string bundleName = _bundleServices.GetBundleName(assetPath);
			return _bundleServices.GetBundleInfo(bundleName);
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
		/// 获取调试汇总信息
		/// </summary>
		public static void GetDebugSummy(DebugSummy summy)
		{
			if (summy == null)
				YooLogger.Error($"{nameof(DebugSummy)} is null");

			AssetSystem.GetDebugSummy(summy);
		}


		#region 资源加载接口
		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public static AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : class
		{
			return LoadAssetInternal(location, typeof(TObject), true);
		}
		public static AssetOperationHandle LoadAssetSync(System.Type type, string location)
		{
			return LoadAssetInternal(location, type, true);
		}

		/// <summary>
		/// 同步加载子资源对象集合
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public static AssetOperationHandle LoadSubAssetsSync<TObject>(string location)
		{
			return LoadSubAssetsInternal(location, typeof(TObject), true);
		}
		public static AssetOperationHandle LoadSubAssetsSync(System.Type type, string location)
		{
			return LoadSubAssetsInternal(location, type, true);
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		public static AssetOperationHandle LoadSceneAsync(string location, SceneInstanceParam instanceParam)
		{
			string scenePath = ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadSceneAsync(scenePath, instanceParam);
			return handle;
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public static AssetOperationHandle LoadAssetAsync<TObject>(string location)
		{
			return LoadAssetInternal(location, typeof(TObject), false);
		}
		public static AssetOperationHandle LoadAssetAsync(System.Type type, string location)
		{
			return LoadAssetInternal(location, type, false);
		}

		/// <summary>
		/// 异步加载子资源对象集合
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public static AssetOperationHandle LoadSubAssetsAsync<TObject>(string location)
		{
			return LoadSubAssetsInternal(location, typeof(TObject), false);
		}
		public static AssetOperationHandle LoadSubAssetsAsync(System.Type type, string location)
		{
			return LoadSubAssetsInternal(location, type, false);
		}

		private static AssetOperationHandle LoadAssetInternal(string location, System.Type assetType, bool waitForAsyncComplete)
		{
			string assetPath = ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadAssetAsync(assetPath, assetType);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		private static AssetOperationHandle LoadSubAssetsInternal(string location, System.Type assetType, bool waitForAsyncComplete)
		{
			string assetPath = ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadSubAssetsAsync(assetPath, assetType);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 资源下载接口
		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static DownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
		{
			return CreatePatchDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static DownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new DownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new DownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.CreateDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建资源包下载器
		/// </summary>
		/// <param name="locations">资源列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static DownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new DownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new DownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");

				List<string> assetPaths = new List<string>(locations.Length);
				foreach (var location in locations)
				{
					string assetPath = ConvertLocationToAssetPath(location);
					assetPaths.Add(assetPath);
				}
				return _hostPlayModeImpl.CreateDownloaderByPaths(assetPaths, downloadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 资源解压接口
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static DownloaderOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			return CreatePatchUnpacker(new string[] { tag }, unpackingMaxNumber, failedTryAgain);
		}
		
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static DownloaderOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new DownloaderOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new DownloaderOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("YooAsset is not initialized.");
				return _hostPlayModeImpl.CreateUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 沙盒相关
		/// <summary>
		/// 清空沙盒目录
		/// 注意：可以使用该方法修复我们本地的客户端
		/// </summary>
		public static void ClearSandbox()
		{
			YooLogger.Warning("Clear sandbox.");
			SandboxHelper.ClearSandbox();
		}

		/// <summary>
		/// 获取沙盒文件夹的路径
		/// </summary>
		public static string GetSandboxRoot()
		{
			return PathHelper.MakePersistentRootPath();
		}
		#endregion

		#region 内部方法
		/// <summary>
		/// 更新资源系统
		/// </summary>
		internal static void InternalUpdate()
		{
			// 更新异步请求操作
			OperationSystem.Update();

			// 更新下载管理系统
			DownloadSystem.Update();

			// 轮询更新资源系统
			AssetSystem.Update();

			// 自动释放零引用资源
			if (_releaseCD > 0)
			{
				_releaseTimer += UnityEngine.Time.unscaledDeltaTime;
				if (_releaseTimer >= _releaseCD)
				{
					_releaseTimer = 0f;
					AssetSystem.UnloadUnusedAssets();
				}
			}
		}

		/// <summary>
		/// 定位地址转换为资源路径
		/// </summary>
		private static string ConvertLocationToAssetPath(string location)
		{
			if (_playMode == EPlayMode.EditorPlayMode)
			{
				string filePath = PathHelper.CombineAssetPath(_locationRoot, location);
				return PathHelper.FindDatabaseAssetPath(filePath);
			}
			else
			{
				return PathHelper.CombineAssetPath(_locationRoot, location);
			}
		}
		#endregion
	}
}