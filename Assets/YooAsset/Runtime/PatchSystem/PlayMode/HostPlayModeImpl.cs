using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	internal class HostPlayModeImpl : IBundleServices
	{
		// 补丁清单
		internal PatchManifest LocalPatchManifest { private set; get; }

		// 参数相关
		private bool _locationToLower;
		private string _defaultHostServer;
		private string _fallbackHostServer;
		private IQueryServices _queryServices;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool locationToLower, string defaultHostServer, string fallbackHostServer, IQueryServices queryServices, string packageName)
		{
			_locationToLower = locationToLower;
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;
			_queryServices = queryServices;

			var operation = new HostPlayModeInitializationOperation(this, packageName);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 获取包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			if (LocalPatchManifest == null)
				return string.Empty;
			return LocalPatchManifest.PackageVersion;
		}

		/// <summary>
		/// 异步更新资源版本号
		/// </summary>
		public UpdatePackageVersionOperation UpdatePackageVersionAsync(string packageName, int timeout)
		{
			var operation = new HostPlayModeUpdatePackageVersionOperation(this, packageName, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新补丁清单
		/// </summary>
		public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageName, string packageVersion, int timeout)
		{
			var operation = new HostPlayModeUpdatePackageManifestOperation(this, packageName, packageVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 检查本地包裹内容的完整性
		/// </summary>
		public CheckPackageContentsOperation CheckPackageContentsAsync(string packageName)
		{
			var operation = new HostPlayModeCheckPackageContentsOperation(this, packageName);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新资源包裹
		/// </summary>
		public DownloadPackageOperation DownloadPackageAsync(string packageName, string packageVersion, int timeout)
		{
			var operation = new HostPlayModeDownloadPackageOperation(this, packageName, packageVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByAll(int fileLoadingMaxNumber, int failedTryAgain, int timeout)
		{
			YooLogger.Log($"Create patch downloader : {LocalPatchManifest.PackageName} {LocalPatchManifest.PackageVersion}");
			List<BundleInfo> downloadList = GetDownloadListByAll();
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain, timeout);
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
				if (IsBuildinPatchBundle(patchBundle))
					continue;

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByTags(string[] tags, int fileLoadingMaxNumber, int failedTryAgain, int timeout)
		{
			YooLogger.Log($"Create patch downloader : {LocalPatchManifest.PackageName} {LocalPatchManifest.PackageVersion}");
			List<BundleInfo> downloadList = GetDownloadListByTags(tags);
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain, timeout);
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
				if (IsBuildinPatchBundle(patchBundle))
					continue;

				// 如果未带任何标记，则统一下载
				if (patchBundle.HasAnyTags() == false)
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
		public PatchDownloaderOperation CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int fileLoadingMaxNumber, int failedTryAgain, int timeout)
		{
			YooLogger.Log($"Create patch downloader : {LocalPatchManifest.PackageName} {LocalPatchManifest.PackageVersion}");
			List<BundleInfo> downloadList = GetDownloadListByPaths(assetInfos);
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain, timeout);
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
				if (IsBuildinPatchBundle(patchBundle))
					continue;

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByTags(string[] tags, int fileUpackingMaxNumber, int failedTryAgain, int timeout)
		{
			YooLogger.Log($"Create patch unpacker : {LocalPatchManifest.PackageName} {LocalPatchManifest.PackageVersion}");
			List<BundleInfo> unpcakList = GetUnpackListByTags(tags);
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
		private List<BundleInfo> GetUnpackListByTags(string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 查询DLC资源
				if (IsBuildinPatchBundle(patchBundle))
				{
					if (patchBundle.HasTag(tags))
					{
						downloadList.Add(patchBundle);
					}
				}
			}

			return ConvertToUnpackList(downloadList);
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByAll(int fileUpackingMaxNumber, int failedTryAgain, int timeout)
		{
			YooLogger.Log($"Create patch unpacker : {LocalPatchManifest.PackageName} {LocalPatchManifest.PackageVersion}");
			List<BundleInfo> unpcakList = GetUnpackListByAll();
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
		private List<BundleInfo> GetUnpackListByAll()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				if (IsBuildinPatchBundle(patchBundle))
				{
					downloadList.Add(patchBundle);
				}
			}

			return ConvertToUnpackList(downloadList);
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
		private BundleInfo ConvertToDownloadInfo(PatchBundle patchBundle)
		{
			string remoteMainURL = GetPatchDownloadMainURL(patchBundle.FileName);
			string remoteFallbackURL = GetPatchDownloadFallbackURL(patchBundle.FileName);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		// 解压相关
		public List<BundleInfo> ConvertToUnpackList(List<PatchBundle> unpackList)
		{
			List<BundleInfo> result = new List<BundleInfo>(unpackList.Count);
			foreach (var patchBundle in unpackList)
			{
				var bundleInfo = ConvertToUnpackInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result;
		}
		public static BundleInfo ConvertToUnpackInfo(PatchBundle patchBundle)
		{
			// 注意：我们把流加载路径指定为远端下载地址
			string streamingPath = PathHelper.ConvertToWWWPath(patchBundle.StreamingFilePath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming, streamingPath, streamingPath);
			return bundleInfo;
		}

		internal List<VerifyInfo> GetVerifyInfoList(bool weaklyUpdateMode)
		{
			List<VerifyInfo> result = new List<VerifyInfo>(LocalPatchManifest.BundleList.Count);

			// 遍历所有文件然后验证并缓存合法文件
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 注意：在弱联网模式下，我们需要验证指定资源版本的所有资源完整性
				if (weaklyUpdateMode)
				{
					bool isBuildinFile = IsBuildinPatchBundle(patchBundle);
					VerifyInfo verifyInfo = new VerifyInfo(isBuildinFile, patchBundle);
					result.Add(verifyInfo);
				}
				else
				{
					string filePath = patchBundle.CachedFilePath;
					if (File.Exists(filePath))
					{
						bool isBuildinFile = IsBuildinPatchBundle(patchBundle);
						VerifyInfo verifyInfo = new VerifyInfo(isBuildinFile, patchBundle);
						result.Add(verifyInfo);
					}
				}
			}

			return result;
		}
		internal void SetLocalPatchManifest(PatchManifest patchManifest)
		{
			LocalPatchManifest = patchManifest;
			LocalPatchManifest.InitAssetPathMapping(_locationToLower);
		}
		internal bool IsBuildinPatchBundle(PatchBundle patchBundle)
		{
			return _queryServices.QueryStreamingAssets(patchBundle.FileName);
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
			if (IsBuildinPatchBundle(patchBundle))
			{
				BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
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
			return LocalPatchManifest.GetAssetsInfoByTags(tags);
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
		string IBundleServices.TryMappingToAssetPath(string location)
		{
			return LocalPatchManifest.TryMappingToAssetPath(location);
		}
		string IBundleServices.GetPackageName()
		{
			return LocalPatchManifest.PackageName;
		}
		bool IBundleServices.IsIncludeBundleFile(string fileName)
		{
			return LocalPatchManifest.IsIncludeBundleFile(fileName);
		}
		#endregion
	}
}