using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal class AssetSystemImpl
	{
		private readonly List<BundleLoaderBase> _loaders = new List<BundleLoaderBase>(1000);
		private readonly List<ProviderBase> _providers = new List<ProviderBase>(1000);
		private readonly static Dictionary<string, SceneOperationHandle> _sceneHandles = new Dictionary<string, SceneOperationHandle>(100);
		private static long _sceneCreateCount = 0;

		private bool _simulationOnEditor;
		private int _loadingMaxNumber;
		public IDecryptionServices DecryptionServices { private set; get; }
		public IBundleServices BundleServices { private set; get; }


		/// <summary>
		/// 初始化
		/// 注意：在使用AssetSystem之前需要初始化
		/// </summary>
		public void Initialize(bool simulationOnEditor, int loadingMaxNumber, IDecryptionServices decryptionServices, IBundleServices bundleServices)
		{
			_simulationOnEditor = simulationOnEditor;
			_loadingMaxNumber = loadingMaxNumber;
			DecryptionServices = decryptionServices;
			BundleServices = bundleServices;
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void Update()
		{
			// 更新加载器	
			foreach (var loader in _loaders)
			{
				loader.Update();
			}

			// 更新资源提供者
			// 注意：循环更新的时候，可能会扩展列表
			// 注意：不能限制场景对象的加载
			int loadingCount = 0;
			for (int i = 0; i < _providers.Count; i++)
			{
				var provider = _providers[i];
				if (provider.IsSceneProvider())
				{
					provider.Update();
				}
				else
				{
					if (loadingCount < _loadingMaxNumber)
						provider.Update();

					if (provider.IsDone == false)
						loadingCount++;
				}
			}
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public void DestroyAll()
		{
			foreach (var provider in _providers)
			{
				provider.Destroy();
			}
			foreach (var loader in _loaders)
			{
				loader.Destroy(true);
			}

			_providers.Clear();
			_loaders.Clear();
			ClearSceneHandle();

			DecryptionServices = null;
			BundleServices = null;
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public void UnloadUnusedAssets()
		{
			// 注意：资源包之间可能存在多层深层嵌套，需要多次循环释放。
			int loopCount = 10;
			for (int i = 0; i < loopCount; i++)
			{
				UnloadUnusedAssetsInternal();
			}
		}
		private void UnloadUnusedAssetsInternal()
		{
			if (_simulationOnEditor)
			{
				for (int i = _providers.Count - 1; i >= 0; i--)
				{
					if (_providers[i].CanDestroy())
					{
						_providers[i].Destroy();
						_providers.RemoveAt(i);
					}
				}
			}
			else
			{
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					BundleLoaderBase loader = _loaders[i];
					loader.TryDestroyAllProviders();
				}
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					BundleLoaderBase loader = _loaders[i];
					if (loader.CanDestroy())
					{
						loader.Destroy(false);
						_loaders.RemoveAt(i);
					}
				}
			}
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceUnloadAllAssets()
		{
			foreach (var provider in _providers)
			{
				provider.Destroy();
			}
			foreach (var loader in _loaders)
			{
				loader.Destroy(true);
			}

			_providers.Clear();
			_loaders.Clear();
			ClearSceneHandle();

			// 注意：调用底层接口释放所有资源
			Resources.UnloadUnusedAssets();
		}

		/// <summary>
		/// 加载场景
		/// </summary>
		public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load scene ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<SceneOperationHandle>();
			}

			// 如果加载的是主场景，则卸载所有缓存的场景
			if (sceneMode == LoadSceneMode.Single)
			{
				UnloadAllScene();
			}

			// 注意：同一个场景的ProviderGUID每次加载都会变化
			string providerGUID = $"{assetInfo.GUID}-{++_sceneCreateCount}";
			ProviderBase provider;
			{
				if (_simulationOnEditor)
					provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
				else
					provider = new BundledSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}

			var handle = provider.CreateHandle<SceneOperationHandle>();
			handle.PackageName = BundleServices.GetPackageName();
			_sceneHandles.Add(providerGUID, handle);
			return handle;
		}

		/// <summary>
		/// 加载资源对象
		/// </summary>
		public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load asset ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<AssetOperationHandle>();
			}

			string providerGUID = assetInfo.GUID;
			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseAssetProvider(this, providerGUID, assetInfo);
				else
					provider = new BundledAssetProvider(this, providerGUID, assetInfo);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle<AssetOperationHandle>();
		}

		/// <summary>
		/// 加载子资源对象
		/// </summary>
		public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load sub assets ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<SubAssetsOperationHandle>();
			}

			string providerGUID = assetInfo.GUID;
			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseSubAssetsProvider(this, providerGUID, assetInfo);
				else
					provider = new BundledSubAssetsProvider(this, providerGUID, assetInfo);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle<SubAssetsOperationHandle>();
		}

		/// <summary>
		/// 加载原生文件
		/// </summary>
		public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load raw file ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<RawFileOperationHandle>();
			}

			string providerGUID = assetInfo.GUID;
			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseRawFileProvider(this, providerGUID, assetInfo);
				else
					provider = new BundledRawFileProvider(this, providerGUID, assetInfo);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle<RawFileOperationHandle>();
		}

		internal void UnloadSubScene(ProviderBase provider)
		{
			string providerGUID = provider.ProviderGUID;
			if (_sceneHandles.ContainsKey(providerGUID) == false)
				throw new Exception("Should never get here !");

			// 释放子场景句柄
			_sceneHandles[providerGUID].ReleaseInternal();
			_sceneHandles.Remove(providerGUID);

			// 卸载未被使用的资源（包括场景）
			UnloadUnusedAssets();
		}
		internal void UnloadAllScene()
		{
			// 释放所有场景句柄
			foreach (var valuePair in _sceneHandles)
			{
				valuePair.Value.ReleaseInternal();
			}
			_sceneHandles.Clear();

			// 卸载未被使用的资源（包括场景）
			UnloadUnusedAssets();
		}
		internal void ClearSceneHandle()
		{
			// 释放资源包下的所有场景
			string packageName = BundleServices.GetPackageName();
			List<string> removeList = new List<string>();
			foreach (var valuePair in _sceneHandles)
			{
				if (valuePair.Value.PackageName == packageName)
				{
					removeList.Add(valuePair.Key);
				}
			}

			foreach (var key in removeList)
			{
				_sceneHandles.Remove(key);
			}
		}

		internal BundleLoaderBase CreateOwnerAssetBundleLoader(AssetInfo assetInfo)
		{
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(assetInfo);
			return CreateAssetBundleLoaderInternal(bundleInfo);
		}
		internal List<BundleLoaderBase> CreateDependAssetBundleLoaders(AssetInfo assetInfo)
		{
			BundleInfo[] depends = BundleServices.GetAllDependBundleInfos(assetInfo);
			List<BundleLoaderBase> result = new List<BundleLoaderBase>(depends.Length);
			foreach (var bundleInfo in depends)
			{
				BundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(bundleInfo);
				result.Add(dependLoader);
			}
			return result;
		}
		internal void RemoveBundleProviders(List<ProviderBase> providers)
		{
			foreach (var provider in providers)
			{
				_providers.Remove(provider);
			}
		}

		private BundleLoaderBase CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			BundleLoaderBase loader = TryGetAssetBundleLoader(bundleInfo.Bundle.BundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
#if UNITY_WEBGL
			loader = new AssetBundleWebLoader(this, bundleInfo);
#else
			if (bundleInfo.Bundle.IsRawFile)
				loader = new RawBundleFileLoader(this, bundleInfo);
			else
				loader = new AssetBundleFileLoader(this, bundleInfo);
#endif

			_loaders.Add(loader);
			return loader;
		}
		private BundleLoaderBase TryGetAssetBundleLoader(string bundleName)
		{
			BundleLoaderBase loader = null;
			for (int i = 0; i < _loaders.Count; i++)
			{
				BundleLoaderBase temp = _loaders[i];
				if (temp.MainBundleInfo.Bundle.BundleName.Equals(bundleName))
				{
					loader = temp;
					break;
				}
			}
			return loader;
		}
		private ProviderBase TryGetProvider(string providerGUID)
		{
			ProviderBase provider = null;
			for (int i = 0; i < _providers.Count; i++)
			{
				ProviderBase temp = _providers[i];
				if (temp.ProviderGUID.Equals(providerGUID))
				{
					provider = temp;
					break;
				}
			}
			return provider;
		}

		#region 调试信息
		internal List<DebugProviderInfo> GetDebugReportInfos()
		{
			List<DebugProviderInfo> result = new List<DebugProviderInfo>(_providers.Count);
			foreach (var provider in _providers)
			{
				DebugProviderInfo providerInfo = new DebugProviderInfo();
				providerInfo.AssetPath = provider.MainAssetInfo.AssetPath;
				providerInfo.SpawnScene = provider.SpawnScene;
				providerInfo.SpawnTime = provider.SpawnTime;
				providerInfo.LoadingTime = provider.LoadingTime;
				providerInfo.RefCount = provider.RefCount;
				providerInfo.Status = provider.Status.ToString();
				providerInfo.DependBundleInfos = new List<DebugBundleInfo>();
				result.Add(providerInfo);

				if (provider is BundledProvider)
				{
					BundledProvider temp = provider as BundledProvider;
					temp.GetBundleDebugInfos(providerInfo.DependBundleInfos);
				}
			}
			return result;
		}
		internal List<BundleInfo> GetLoadedBundleInfos()
		{
			List<BundleInfo> result = new List<BundleInfo>(100);
			foreach (var bundleLoader in _loaders)
			{
				result.Add(bundleLoader.MainBundleInfo);
			}
			return result;
		}
		#endregion
	}
}