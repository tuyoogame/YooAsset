using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal static class AssetSystem
	{
		private static readonly List<BundleFileLoader> _loaders = new List<BundleFileLoader>(1000);
		private static readonly List<AssetProviderBase> _providers = new List<AssetProviderBase>(1000);

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

			// 注意：需要立刻卸载场景
			if (SimulationOnEditor)
			{
				for (int i = _providers.Count - 1; i >= 0; i--)
				{
					AssetProviderBase provider = _providers[i];
					if (provider.IsSceneProvider() && provider.CanDestroy())
					{
						provider.Destory();
						_providers.RemoveAt(i);
					}
				}
			}
			else
			{
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					BundleFileLoader loader = _loaders[i];
					if (loader.IsSceneLoader())
					{
						loader.TryDestroyAllProviders();
					}
				}
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					BundleFileLoader loader = _loaders[i];
					if (loader.IsSceneLoader() && loader.CanDestroy())
					{
						loader.Destroy(false);
						_loaders.RemoveAt(i);
					}
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
					BundleFileLoader loader = _loaders[i];
					loader.TryDestroyAllProviders();
				}
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					BundleFileLoader loader = _loaders[i];
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
		/// <param name="scenePath">场景名称</param>
		public static AssetOperationHandle LoadSceneAsync(string scenePath, SceneInstanceParam instanceParam)
		{
			AssetProviderBase provider = TryGetAssetProvider(scenePath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseSceneProvider(scenePath, instanceParam);
				else
					provider = new BundledSceneProvider(scenePath, instanceParam);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <param name="assetType">资源类型</param>
		public static AssetOperationHandle LoadAssetAsync(string assetPath, System.Type assetType)
		{
			AssetProviderBase provider = TryGetAssetProvider(assetPath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseAssetProvider(assetPath, assetType);
				else
					provider = new BundledAssetProvider(assetPath, assetType);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载所有子资源对象
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <param name="assetType">资源类型</param>、
		public static AssetOperationHandle LoadSubAssetsAsync(string assetPath, System.Type assetType)
		{
			AssetProviderBase provider = TryGetAssetProvider(assetPath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new DatabaseSubAssetsProvider(assetPath, assetType);
				else
					provider = new BundledSubAssetsProvider(assetPath, assetType);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}


		internal static BundleFileLoader CreateOwnerBundleLoader(string assetPath)
		{
			string bundleName = BundleServices.GetBundleName(assetPath);
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(bundleName);
			return CreateBundleFileLoaderInternal(bundleInfo);
		}
		internal static List<BundleFileLoader> CreateDependBundleLoaders(string assetPath)
		{
			List<BundleFileLoader> result = new List<BundleFileLoader>();
			string[] depends = BundleServices.GetAllDependencies(assetPath);
			if (depends != null)
			{
				foreach (var dependBundleName in depends)
				{
					BundleInfo dependBundleInfo = BundleServices.GetBundleInfo(dependBundleName);
					BundleFileLoader dependLoader = CreateBundleFileLoaderInternal(dependBundleInfo);
					result.Add(dependLoader);
				}
			}
			return result;
		}
		internal static void RemoveBundleProviders(List<AssetProviderBase> providers)
		{
			foreach (var provider in providers)
			{
				_providers.Remove(provider);
			}
		}

		private static BundleFileLoader CreateBundleFileLoaderInternal(BundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			BundleFileLoader loader = TryGetBundleFileLoader(bundleInfo.BundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
			loader = new BundleFileLoader(bundleInfo);
			_loaders.Add(loader);
			return loader;
		}
		private static BundleFileLoader TryGetBundleFileLoader(string bundleName)
		{
			BundleFileLoader loader = null;
			for (int i = 0; i < _loaders.Count; i++)
			{
				BundleFileLoader temp = _loaders[i];
				if (temp.BundleFileInfo.BundleName.Equals(bundleName))
				{
					loader = temp;
					break;
				}
			}
			return loader;
		}
		private static AssetProviderBase TryGetAssetProvider(string assetPath)
		{
			AssetProviderBase provider = null;
			for (int i = 0; i < _providers.Count; i++)
			{
				AssetProviderBase temp = _providers[i];
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