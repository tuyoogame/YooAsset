﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal static class AssetSystem
	{
		private static readonly List<AssetBundleLoaderBase> _loaders = new List<AssetBundleLoaderBase>(1000);
		private static readonly List<ProviderBase> _providers = new List<ProviderBase>(1000);
		private static readonly Dictionary<string, SceneOperationHandle> _sceneHandles = new Dictionary<string, SceneOperationHandle>(100);
		
		/// <summary>
		/// 在编辑器下模拟运行
		/// </summary>
		public static bool SimulationOnEditor { private set; get; }

		/// <summary>
		/// 运行时的最大加载个数
		/// </summary>
		public static int AssetLoadingMaxNumber { private set; get; }

		public static IDecryptionServices DecryptionServices { private set; get; }
		public static IBundleServices BundleServices { private set; get; }


		/// <summary>
		/// 初始化资源系统
		/// 注意：在使用AssetSystem之前需要初始化
		/// </summary>
		public static void Initialize(bool simulationOnEditor, int assetLoadingMaxNumber, IDecryptionServices decryptionServices, IBundleServices bundleServices)
		{
			SimulationOnEditor = simulationOnEditor;
			AssetLoadingMaxNumber = assetLoadingMaxNumber;
			DecryptionServices = decryptionServices;
			BundleServices = bundleServices;
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public static void Update()
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
					if (loadingCount < AssetLoadingMaxNumber)
						provider.Update();

					if (provider.IsDone == false)
						loadingCount++;
				}
			}
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public static void UnloadUnusedAssets()
		{
			if (SimulationOnEditor)
			{
				for (int i = _providers.Count - 1; i >= 0; i--)
				{
					if (_providers[i].CanDestroy())
					{
						_providers[i].Destory();
						_providers.RemoveAt(i);
					}
				}
			}
			else
			{
				for (int i = _loaders.Count-1; i >= 0; i--) {
					AssetBundleLoaderBase loader = _loaders[i];
					loader.TryDestroyAllProviders();
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
		public static void ForceUnloadAllAssets()
		{
			foreach (var provider in _providers)
			{
				provider.Destory();
			}
			_providers.Clear();

			foreach (var loader in _loaders)
			{
				loader.Destroy(true);
			}
			_loaders.Clear();

			// 注意：调用底层接口释放所有资源
			Resources.UnloadUnusedAssets();
		}


		/// <summary>
		/// 异步加载场景
		/// </summary>
		public static SceneOperationHandle LoadSceneAsync(string scenePath, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
		{
			// 注意：场景句柄永远保持唯一
			if (_sceneHandles.ContainsKey(scenePath))
				return _sceneHandles[scenePath];

			// 如果加载的是主场景，则卸载所有缓存的场景
			if (sceneMode == LoadSceneMode.Single)
			{
				UnloadAllScene();
			}

			ProviderBase provider = TryGetProvider(scenePath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseSceneProvider(scenePath, sceneMode, activateOnLoad, priority);
				else
					provider = new BundledSceneProvider(scenePath, sceneMode, activateOnLoad, priority);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}

			var handle = provider.CreateHandle() as SceneOperationHandle;
			_sceneHandles.Add(scenePath, handle);
			return handle;
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		public static AssetOperationHandle LoadAssetAsync(string assetPath, System.Type assetType)
		{
			ProviderBase provider = TryGetProvider(assetPath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseAssetProvider(assetPath, assetType);
				else
					provider = new BundledAssetProvider(assetPath, assetType);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle() as AssetOperationHandle;
		}

		/// <summary>
		/// 异步加载所有子资源对象
		/// </summary>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(string assetPath, System.Type assetType)
		{
			ProviderBase provider = TryGetProvider(assetPath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseSubAssetsProvider(assetPath, assetType);
				else
					provider = new BundledSubAssetsProvider(assetPath, assetType);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle() as SubAssetsOperationHandle;
		}


		internal static void UnloadSubScene(ProviderBase provider)
		{
			string scenePath = provider.AssetPath;
			if (_sceneHandles.ContainsKey(scenePath) == false)
				throw new Exception("Should never get here !");

			// 释放子场景句柄
			_sceneHandles[scenePath].ReleaseInternal();
			_sceneHandles.Remove(scenePath);

			// 卸载未被使用的资源（包括场景）
			AssetSystem.UnloadUnusedAssets();

			// 检验子场景是否销毁
			if (provider.IsDestroyed == false)
			{
				throw new Exception("Should never get here !");
			}
		}
		internal static void UnloadAllScene()
		{
			// 释放所有场景句柄
			foreach (var valuePair in _sceneHandles)
			{
				valuePair.Value.ReleaseInternal();
			}
			_sceneHandles.Clear();

			// 卸载未被使用的资源（包括场景）
			AssetSystem.UnloadUnusedAssets();

			// 检验所有场景是否销毁
			foreach (var provider in _providers)
			{
				if (provider.IsSceneProvider())
				{
					if (provider.IsDestroyed == false)
						throw new Exception("Should never get here !");
				}
			}
		}

		internal static AssetBundleLoaderBase CreateOwnerAssetBundleLoader(string assetPath)
		{
			string bundleName = BundleServices.GetBundleName(assetPath);
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(bundleName);
			return CreateAssetBundleLoaderInternal(bundleInfo);
		}
		internal static List<AssetBundleLoaderBase> CreateDependAssetBundleLoaders(string assetPath)
		{
			List<AssetBundleLoaderBase> result = new List<AssetBundleLoaderBase>();
			string[] depends = BundleServices.GetAllDependencies(assetPath);
			if (depends != null)
			{
				foreach (var dependBundleName in depends)
				{
					BundleInfo dependBundleInfo = BundleServices.GetBundleInfo(dependBundleName);
					AssetBundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(dependBundleInfo);
					result.Add(dependLoader);
				}
			}
			return result;
		}
		internal static void RemoveBundleProviders(List<ProviderBase> providers)
		{
			foreach (var provider in providers)
			{
				_providers.Remove(provider);
			}
		}

		private static AssetBundleLoaderBase CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			AssetBundleLoaderBase loader = TryGetAssetBundleLoader(bundleInfo.BundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
#if UNITY_WEBGL
			loader = new AssetBundleWebLoader(bundleInfo);
#else
			loader = new AssetBundleFileLoader(bundleInfo);
#endif

			_loaders.Add(loader);
			return loader;
		}
		private static AssetBundleLoaderBase TryGetAssetBundleLoader(string bundleName)
		{
			AssetBundleLoaderBase loader = null;
			for (int i = 0; i < _loaders.Count; i++)
			{
				AssetBundleLoaderBase temp = _loaders[i];
				if (temp.BundleFileInfo.BundleName.Equals(bundleName))
				{
					loader = temp;
					break;
				}
			}
			return loader;
		}
		private static ProviderBase TryGetProvider(string assetPath)
		{
			ProviderBase provider = null;
			for (int i = 0; i < _providers.Count; i++)
			{
				ProviderBase temp = _providers[i];
				if (temp.AssetPath.Equals(assetPath))
				{
					provider = temp;
					break;
				}
			}
			return provider;
		}


		#region 调试专属方法
		internal static void GetDebugReport(DebugReport report)
		{
			report.ClearAll();
			report.BundleCount = _loaders.Count;
			report.AssetCount = _providers.Count;

			foreach (var provider in _providers)
			{
				DebugProviderInfo providerInfo = new DebugProviderInfo();
				providerInfo.AssetPath = provider.AssetPath;
				providerInfo.SpawnScene = provider.SpawnScene;
				providerInfo.SpawnTime = provider.SpawnTime;
				providerInfo.RefCount = provider.RefCount;
				providerInfo.Status = provider.Status;
				providerInfo.BundleInfos.Clear();
				report.ProviderInfos.Add(providerInfo);

				if (provider is BundledProvider)
				{
					BundledProvider temp = provider as BundledProvider;
					temp.GetBundleDebugInfos(providerInfo.BundleInfos);
				}
			}

			// 重新排序
			report.ProviderInfos.Sort();
		}
		#endregion
	}
}