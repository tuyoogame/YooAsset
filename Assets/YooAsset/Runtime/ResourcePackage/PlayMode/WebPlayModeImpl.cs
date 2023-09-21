using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class WebPlayModeImpl : IPlayMode, IBundleQuery
	{
		private PackageManifest _activeManifest;
		private ResourceAssist _assist;
		private IBuildinQueryServices _buildinQueryServices;
		private IRemoteServices _remoteServices;

		public readonly string PackageName;
		public PersistentManager Persistent
		{
			get { return _assist.Persistent; }
		}
		public IRemoteServices RemoteServices
		{
			get { return _remoteServices; }
		}


		public WebPlayModeImpl(string packageName)
		{
			PackageName = packageName;
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(ResourceAssist assist, IBuildinQueryServices buildinQueryServices, IRemoteServices remoteServices)
		{
			_assist = assist;
			_buildinQueryServices = buildinQueryServices;
			_remoteServices = remoteServices;

			var operation = new WebPlayModeInitializationOperation(this);
			OperationSystem.StartOperation(PackageName, operation);
			return operation;
		}

		// 下载相关
		private BundleInfo ConvertToDownloadInfo(PackageBundle packageBundle)
		{
			string remoteMainURL = _remoteServices.GetRemoteMainURL(packageBundle.FileName);
			string remoteFallbackURL = _remoteServices.GetRemoteFallbackURL(packageBundle.FileName);
			BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		// 查询相关
		private bool IsBuildinPackageBundle(PackageBundle packageBundle)
		{
			return _buildinQueryServices.Query(PackageName, packageBundle.FileName);
		}

		#region IPlayModeServices接口
		public PackageManifest ActiveManifest
		{
			set
			{
				_activeManifest = value;
			}
			get
			{
				return _activeManifest;
			}
		}
		public void FlushManifestVersionFile()
		{
		}

		UpdatePackageVersionOperation IPlayMode.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
		{
			var operation = new WebPlayModeUpdatePackageVersionOperation(this, appendTimeTicks, timeout);
			OperationSystem.StartOperation(PackageName, operation);
			return operation;
		}
		UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
		{
			var operation = new WebPlayModeUpdatePackageManifestOperation(this, packageVersion, timeout);
			OperationSystem.StartOperation(PackageName, operation);
			return operation;
		}
		PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
		{
			var operation = new WebPlayModePreDownloadContentOperation(this);
			OperationSystem.StartOperation(PackageName, operation);
			return operation;
		}

		ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(PackageName, downloadingMaxNumber, failedTryAgain, timeout);
		}
		ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(PackageName, downloadingMaxNumber, failedTryAgain, timeout);
		}
		ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(PackageName, downloadingMaxNumber, failedTryAgain, timeout);
		}

		ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceUnpackerOperation.CreateEmptyUnpacker(PackageName, upackingMaxNumber, failedTryAgain, timeout);
		}
		ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceUnpackerOperation.CreateEmptyUnpacker(PackageName, upackingMaxNumber, failedTryAgain, timeout);
		}

		ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceImporterOperation.CreateEmptyImporter(PackageName, importerMaxNumber, failedTryAgain, timeout);
		}
		#endregion

		#region IBundleQuery接口
		private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
		{
			if (packageBundle == null)
				throw new Exception("Should never get here !");

			// 查询APP资源
			if (IsBuildinPackageBundle(packageBundle))
			{
				BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}

			// 从服务端下载
			return ConvertToDownloadInfo(packageBundle);
		}
		BundleInfo IBundleQuery.GetMainBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果清单里未找到资源包会抛出异常！
			var packageBundle = _activeManifest.GetMainPackageBundle(assetInfo.AssetPath);
			return CreateBundleInfo(packageBundle);
		}
		BundleInfo[] IBundleQuery.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果清单里未找到资源包会抛出异常！
			var depends = _activeManifest.GetAllDependencies(assetInfo.AssetPath);
			List<BundleInfo> result = new List<BundleInfo>(depends.Length);
			foreach (var packageBundle in depends)
			{
				BundleInfo bundleInfo = CreateBundleInfo(packageBundle);
				result.Add(bundleInfo);
			}
			return result.ToArray();
		}
		bool IBundleQuery.ManifestValid()
		{
			return _activeManifest != null;
		}
		#endregion
	}
}