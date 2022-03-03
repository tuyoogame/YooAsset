using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class HostPlayModeImpl : IBundleServices
	{
		// 补丁清单
		internal PatchManifest AppPatchManifest;
		internal PatchManifest LocalPatchManifest;

		// 参数相关
		internal bool ClearCacheWhenDirty { private set; get; }
		internal bool IgnoreResourceVersion { private set; get; }
		private string _defaultHostServer;
		private string _fallbackHostServer;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool clearCacheWhenDirty, bool ignoreResourceVersion,
			 string defaultHostServer, string fallbackHostServer)
		{
			ClearCacheWhenDirty = clearCacheWhenDirty;
			IgnoreResourceVersion = ignoreResourceVersion;
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;

			var operation = new HostPlayModeInitializationOperation(this);
			OperationUpdater.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新补丁清单
		/// </summary>
		public UpdateManifestOperation UpdatePatchManifestAsync(int updateResourceVersion, int timeout)
		{
			var operation = new HostPlayModeUpdateManifestOperation(this, updateResourceVersion, timeout);
			OperationUpdater.ProcessOperaiton(operation);
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
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			if (LocalPatchManifest == null)
				return new string[0];
			return LocalPatchManifest.GetBuildinTags();
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public DownloaderOperation CreateDownloaderByTags(string[] tags, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<AssetBundleInfo> downloadList = GetDownloadListByTags(tags);
			var operation = new DownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<AssetBundleInfo> GetDownloadListByTags(string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
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
		public DownloaderOperation CreateDownloaderByPaths(List<string> assetPaths, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<AssetBundleInfo> downloadList = GetDownloadListByPaths(assetPaths);
			var operation = new DownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<AssetBundleInfo> GetDownloadListByPaths(List<string> assetPaths)
		{
			// 获取资源对象的资源包和所有依赖资源包
			List<PatchBundle> checkList = new List<PatchBundle>();
			foreach (var assetPath in assetPaths)
			{
				string mainBundleName = LocalPatchManifest.GetAssetBundleName(assetPath);
				if (string.IsNullOrEmpty(mainBundleName) == false)
				{
					if (LocalPatchManifest.Bundles.TryGetValue(mainBundleName, out PatchBundle mainBundle))
					{
						if (checkList.Contains(mainBundle) == false)
							checkList.Add(mainBundle);
					}
				}

				string[] dependBundleNames = LocalPatchManifest.GetAllDependencies(assetPath);
				foreach (var dependBundleName in dependBundleNames)
				{
					if (LocalPatchManifest.Bundles.TryGetValue(dependBundleName, out PatchBundle dependBundle))
					{
						if (checkList.Contains(dependBundle) == false)
							checkList.Add(dependBundle);
					}
				}
			}

			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in checkList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		// WEB相关
		internal string GetPatchDownloadMainURL(int resourceVersion, string fileName)
		{
			if (IgnoreResourceVersion)
				return $"{_defaultHostServer}/{fileName}";
			else
				return $"{_defaultHostServer}/{resourceVersion}/{fileName}";
		}
		internal string GetPatchDownloadFallbackURL(int resourceVersion, string fileName)
		{
			if (IgnoreResourceVersion)
				return $"{_fallbackHostServer}/{fileName}";
			else
				return $"{_fallbackHostServer}/{resourceVersion}/{fileName}";
		}

		// 下载相关
		private AssetBundleInfo ConvertToDownloadInfo(PatchBundle patchBundle)
		{
			// 注意：资源版本号只用于确定下载路径
			string sandboxPath = PatchHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
			string remoteMainURL = GetPatchDownloadMainURL(patchBundle.Version, patchBundle.Hash);
			string remoteFallbackURL = GetPatchDownloadFallbackURL(patchBundle.Version, patchBundle.Hash);
			AssetBundleInfo bundleInfo = new AssetBundleInfo(patchBundle, sandboxPath, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}
		private List<AssetBundleInfo> ConvertToDownloadList(List<PatchBundle> downloadList)
		{
			List<AssetBundleInfo> result = new List<AssetBundleInfo>(downloadList.Count);
			foreach (var patchBundle in downloadList)
			{
				var bundleInfo = ConvertToDownloadInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result;
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new AssetBundleInfo(string.Empty, string.Empty);

			if (LocalPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				// 查询APP资源
				if (AppPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
					{
						string appLoadPath = AssetPathHelper.MakeStreamingLoadPath(appPatchBundle.Hash);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(appPatchBundle, appLoadPath);
						return bundleInfo;
					}
				}

				// 查询沙盒资源				
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
				{
					string sandboxLoadPath = PatchHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
					AssetBundleInfo bundleInfo = new AssetBundleInfo(patchBundle, sandboxLoadPath);
					return bundleInfo;
				}

				// 从服务端下载
				return ConvertToDownloadInfo(patchBundle);
			}
			else
			{
				Logger.Warning($"Not found bundle in patch manifest : {bundleName}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, string.Empty);
				return bundleInfo;
			}
		}
		string IBundleServices.GetAssetBundleName(string assetPath)
		{
			return LocalPatchManifest.GetAssetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return LocalPatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}