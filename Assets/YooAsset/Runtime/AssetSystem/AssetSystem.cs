using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal static class AssetSystem
	{
		private static readonly List<AssetBundleLoader> _loaders = new List<AssetBundleLoader>(1000);
		private static readonly List<ProviderBase> _providers = new List<ProviderBase>(1000);

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
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					AssetBundleLoader loader = _loaders[i];
					loader.TryDestroyAllProviders();
				}
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					AssetBundleLoader loader = _loaders[i];
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
		/// 异步加载原生文件
		/// </summary>
		public static RawFileOperation LoadRawFileAsync(string assetPath, string savePath)
		{
			string bundleName = BundleServices.GetBundleName(assetPath);
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(bundleName);
			RawFileOperation operation = new RawFileOperation(bundleInfo, savePath);
			OperationSystem.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		public static SceneOperationHandle LoadSceneAsync(string scenePath, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
		{
			ProviderBase provider = TryGetProvider(scenePath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseSceneProvider(scenePath, sceneMode, activateOnLoad, priority);
				else
					provider = new BundledSceneProvider(scenePath, sceneMode, activateOnLoad, priority);
				_providers.Add(provider);
			}
			return provider.CreateHandle() as SceneOperationHandle;
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
				_providers.Add(provider);
			}
			return provider.CreateHandle() as SubAssetsOperationHandle;
		}


		internal static AssetBundleLoader CreateOwnerAssetBundleLoader(string assetPath)
		{
			string bundleName = BundleServices.GetBundleName(assetPath);
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(bundleName);
			return CreateAssetBundleLoaderInternal(bundleInfo);
		}
		internal static List<AssetBundleLoader> CreateDependAssetBundleLoaders(string assetPath)
		{
			List<AssetBundleLoader> result = new List<AssetBundleLoader>();
			string[] depends = BundleServices.GetAllDependencies(assetPath);
			if (depends != null)
			{
				foreach (var dependBundleName in depends)
				{
					BundleInfo dependBundleInfo = BundleServices.GetBundleInfo(dependBundleName);
					AssetBundleLoader dependLoader = CreateAssetBundleLoaderInternal(dependBundleInfo);
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

		private static AssetBundleLoader CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			AssetBundleLoader loader = TryGetAssetBundleLoader(bundleInfo.BundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
			loader = new AssetBundleLoader(bundleInfo);
			_loaders.Add(loader);
			return loader;
		}
		private static AssetBundleLoader TryGetAssetBundleLoader(string bundleName)
		{
			AssetBundleLoader loader = null;
			for (int i = 0; i < _loaders.Count; i++)
			{
				AssetBundleLoader temp = _loaders[i];
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
				providerInfo.RefCount = provider.RefCount;
				providerInfo.Status = (int)provider.Status;
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