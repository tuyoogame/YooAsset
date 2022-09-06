using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	internal class HostPlayModeImpl : IBundleServices
	{
		// 补丁清单
		internal PatchManifest AppPatchManifest { private set; get; }
		internal PatchManifest LocalPatchManifest { private set; get; }

		// 参数相关
		private bool _locationToLower;
		private bool _clearCacheWhenDirty;
		private string _defaultHostServer;
		private string _fallbackHostServer;

		public bool ClearCacheWhenDirty
		{
			get { return _clearCacheWhenDirty; }
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool locationToLower, bool clearCacheWhenDirty, string defaultHostServer, string fallbackHostServer)
		{
			_locationToLower = locationToLower;
			_clearCacheWhenDirty = clearCacheWhenDirty;
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;

			var operation = new HostPlayModeInitializationOperation(this);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新资源版本号
		/// </summary>
		public UpdateStaticVersionOperation UpdateStaticVersionAsync(int timeout)
		{
			var operation = new HostPlayModeUpdateStaticVersionOperation(this, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新补丁清单
		/// </summary>
		public UpdateManifestOperation UpdatePatchManifestAsync(int resourceVersion, int timeout)
		{
			var operation = new HostPlayModeUpdateManifestOperation(this, resourceVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新补丁清单（弱联网）
		/// </summary>
		public UpdateManifestOperation WeaklyUpdatePatchManifestAsync(int resourceVersion)
		{
			var operation = new HostPlayModeWeaklyUpdateManifestOperation(this, resourceVersion);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新资源包裹
		/// </summary>
		public UpdatePackageOperation UpdatePackageAsync(int resourceVersion, int timeout)
		{
			var operation = new HostPlayModeUpdatePackageOperation(this, resourceVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (LocalPatchManifest == null)
				return 0;
			return LocalPatchManifest.ResourceVersion;
		}

		/// <summary>
		/// 获取未被使用的缓存文件路径集合
		/// </summary>
		public List<string> ClearUnusedCacheFilePaths()
		{
			string cacheFolderPath = SandboxHelper.GetCacheFolderPath();
			if (Directory.Exists(cacheFolderPath) == false)
				return new List<string>();

			DirectoryInfo directoryInfo = new DirectoryInfo(cacheFolderPath);
			FileInfo[] fileInfos = directoryInfo.GetFiles();
			List<string> result = new List<string>(fileInfos.Length);
			foreach (FileInfo fileInfo in fileInfos)
			{
				bool used = false;
				foreach (var patchBundle in LocalPatchManifest.BundleList)
				{
					if (fileInfo.Name == patchBundle.FileName)
					{
						used = true;
						break;
					}
				}
				if (used == false)
					result.Add(fileInfo.FullName);
			}
			return result;
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByAll(int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = GetDownloadListByAll();
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetDownloadListByAll()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.TryGetPatchBundle(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Equals(patchBundle))
						continue;
				}

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByTags(string[] tags, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = GetDownloadListByTags(tags);
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetDownloadListByTags(string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.TryGetPatchBundle(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Equals(patchBundle))
						continue;
				}

				// 如果是纯内置资源，则统一下载
				// 注意：可能是新增的或者变化的内置资源
				// 注意：可能是由热更资源转换的内置资源
				if (patchBundle.IsPureBuildin())
				{
					downloadList.Add(patchBundle);
				}
				else
				{
					// 查询DLC资源
					if (patchBundle.HasTag(tags))
					{
						downloadList.Add(patchBundle);
					}
				}
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = GetDownloadListByPaths(assetInfos);
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetDownloadListByPaths(AssetInfo[] assetInfos)
		{
			// 获取资源对象的资源包和所有依赖资源包
			List<PatchBundle> checkList = new List<PatchBundle>();
			foreach (var assetInfo in assetInfos)
			{
				if (assetInfo.IsInvalid)
				{
					YooLogger.Warning(assetInfo.Error);
					continue;
				}

				// 注意：如果补丁清单里未找到资源包会抛出异常！
				PatchBundle mainBundle = LocalPatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
				if (checkList.Contains(mainBundle) == false)
					checkList.Add(mainBundle);

				// 注意：如果补丁清单里未找到资源包会抛出异常！
				PatchBundle[] dependBundles = LocalPatchManifest.GetAllDependencies(assetInfo.AssetPath);
				foreach (var dependBundle in dependBundles)
				{
					if (checkList.Contains(dependBundle) == false)
						checkList.Add(dependBundle);
				}
			}

			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in checkList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.TryGetPatchBundle(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Equals(patchBundle))
						continue;
				}

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByTags(string[] tags, int fileUpackingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> unpcakList = GetUnpackListByTags(tags);
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetUnpackListByTags(string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in AppPatchManifest.BundleList)
			{
				// 如果不是内置资源
				if (patchBundle.IsBuildin == false)
					continue;

				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 查询DLC资源
				if (patchBundle.HasTag(tags))
				{
					downloadList.Add(patchBundle);
				}
			}

			return PatchHelper.ConvertToUnpackList(downloadList);
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByAll(int fileUpackingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> unpcakList = GetUnpackListByAll();
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetUnpackListByAll()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in AppPatchManifest.BundleList)
			{
				// 如果不是内置资源
				if (patchBundle.IsBuildin == false)
					continue;

				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				downloadList.Add(patchBundle);
			}

			return PatchHelper.ConvertToUnpackList(downloadList);
		}

		// WEB相关
		public string GetPatchDownloadMainURL(string fileName)
		{
			return $"{_defaultHostServer}/{fileName}";
		}
		public string GetPatchDownloadFallbackURL(string fileName)
		{
			return $"{_fallbackHostServer}/{fileName}";
		}

		// 下载相关
		public List<BundleInfo> ConvertToDownloadList(List<PatchBundle> downloadList)
		{
			List<BundleInfo> result = new List<BundleInfo>(downloadList.Count);
			foreach (var patchBundle in downloadList)
			{
				var bundleInfo = ConvertToDownloadInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result;
		}
		public BundleInfo ConvertToDownloadInfo(PatchBundle patchBundle)
		{
			string remoteMainURL = GetPatchDownloadMainURL(patchBundle.FileName);
			string remoteFallbackURL = GetPatchDownloadFallbackURL(patchBundle.FileName);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		// 设置资源清单
		internal void SetAppPatchManifest(PatchManifest patchManifest)
		{
			AppPatchManifest = patchManifest;
		}
		internal void SetLocalPatchManifest(PatchManifest patchManifest)
		{
			LocalPatchManifest = patchManifest;
			LocalPatchManifest.InitAssetPathMapping(_locationToLower);
		}

		#region IBundleServices接口
		private BundleInfo CreateBundleInfo(PatchBundle patchBundle)
		{
			if (patchBundle == null)
				throw new Exception("Should never get here !");

			// 查询沙盒资源
			if (CacheSystem.IsCached(patchBundle))
			{
				BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromCache);
				return bundleInfo;
			}

			// 查询APP资源
			if (AppPatchManifest.TryGetPatchBundle(patchBundle.BundleName, out PatchBundle appPatchBundle))
			{
				if (appPatchBundle.IsBuildin && appPatchBundle.Equals(patchBundle))
				{
					BundleInfo bundleInfo = new BundleInfo(appPatchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
					return bundleInfo;
				}
			}

			// 从服务端下载
			return ConvertToDownloadInfo(patchBundle);
		}
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var patchBundle = LocalPatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
			return CreateBundleInfo(patchBundle);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var depends = LocalPatchManifest.GetAllDependencies(assetInfo.AssetPath);
			List<BundleInfo> result = new List<BundleInfo>(depends.Length);
			foreach (var patchBundle in depends)
			{
				BundleInfo bundleInfo = CreateBundleInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result.ToArray();
		}
		AssetInfo[] IBundleServices.GetAssetInfos(string[] tags)
		{
			return PatchHelper.GetAssetsInfoByTags(LocalPatchManifest, tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (LocalPatchManifest.TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return LocalPatchManifest.MappingToAssetPath(location);
		}
		#endregion
	}
}