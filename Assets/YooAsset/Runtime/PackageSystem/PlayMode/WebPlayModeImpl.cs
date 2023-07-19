using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class WebPlayModeImpl : IPlayModeServices, IBundleServices
	{
		private PackageManifest _activeManifest;

		// 参数相关
		private string _packageName;
		private IQueryServices _queryServices;
		private IRemoteServices _remoteServices;

		public IRemoteServices RemoteServices
		{
			get { return _remoteServices; }
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(string packageName, IQueryServices queryServices, IRemoteServices remoteServices)
		{
			_packageName = packageName;
			_queryServices = queryServices;
			_remoteServices = remoteServices;

			var operation = new WebPlayModeInitializationOperation(this, packageName);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		// 下载相关
		private BundleInfo ConvertToDownloadInfo(PackageBundle packageBundle)
		{
			string remoteMainURL = _remoteServices.GetRemoteMainURL(packageBundle.FileName);
			string remoteFallbackURL = _remoteServices.GetRemoteFallbackURL(packageBundle.FileName);
			BundleInfo bundleInfo = new BundleInfo(packageBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		// 查询相关
		private bool IsBuildinPackageBundle(PackageBundle packageBundle)
		{
			return _queryServices.QueryStreamingAssets(_packageName, packageBundle.FileName);
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

		UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
		{
			var operation = new WebPlayModeUpdatePackageVersionOperation(this, _packageName, appendTimeTicks, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}
		UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
		{
			var operation = new WebPlayModeUpdatePackageManifestOperation(this, _packageName, packageVersion, timeout);
			OperationSystem.StartOperation(operation);
			return operation;
		}
		PreDownloadContentOperation IPlayModeServices.PreDownloadContentAsync(string packageVersion, int timeout)
		{
			var operation = new WebPlayModePreDownloadContentOperation();
			OperationSystem.StartOperation(operation);
			return operation;
		}

		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		ResourceDownloaderOperation IPlayModeServices.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		ResourceUnpackerOperation IPlayModeServices.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		ResourceUnpackerOperation IPlayModeServices.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return ResourceUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		#endregion

		#region IBundleServices接口
		private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
		{
			if (packageBundle == null)
				throw new Exception("Should never get here !");

			// 查询APP资源
			if (IsBuildinPackageBundle(packageBundle))
			{
				BundleInfo bundleInfo = new BundleInfo(packageBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}

			// 从服务端下载
			return ConvertToDownloadInfo(packageBundle);
		}
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果清单里未找到资源包会抛出异常！
			var packageBundle = _activeManifest.GetMainPackageBundle(assetInfo.AssetPath);
			return CreateBundleInfo(packageBundle);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
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
		string IBundleServices.GetBundleName(int bundleID)
		{
			return _activeManifest.GetBundleName(bundleID);
		}
		bool IBundleServices.IsServicesValid()
		{
			return _activeManifest != null;
		}
		#endregion
	}
}