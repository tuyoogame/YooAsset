using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class EditorSimulateModeImpl : IPlayModeServices, IBundleServices
	{
		private PatchManifest _activeManifest;
		private bool _locationToLower;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool locationToLower, string simulatePatchManifestPath)
		{
			_locationToLower = locationToLower;
			var operation = new EditorSimulateModeInitializationOperation(this, simulatePatchManifestPath);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		#region IPlayModeServices接口
		public PatchManifest ActiveManifest
		{
			set
			{
				_activeManifest = value;
				_activeManifest.InitAssetPathMapping(_locationToLower);
			}
			get
			{
				return _activeManifest;
			}
		}
		public bool IsBuildinPatchBundle(PatchBundle patchBundle)
		{
			return true;
		}

		UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
		{
			var operation = new EditorPlayModeUpdatePackageVersionOperation();
			OperationSystem.StartOperation(operation);
			return operation;
		}
		UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, int timeout)
		{
			var operation = new EditorPlayModeUpdatePackageManifestOperation();
			OperationSystem.StartOperation(operation);
			return operation;
		}
		PreDownloadPackageOperation IPlayModeServices.PreDownloadPackageAsync(string packageVersion, int timeout)
		{
			var operation = new EditorPlayModePreDownloadPackageOperation();
			OperationSystem.StartOperation(operation);
			return operation;
		}
		
		PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		#endregion

		#region IBundleServices接口
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var patchBundle = _activeManifest.GetMainPatchBundle(assetInfo.AssetPath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromEditor, assetInfo.AssetPath);
			return bundleInfo;
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			throw new NotImplementedException();
		}
		bool IBundleServices.IsServicesValid()
		{
			return _activeManifest != null;
		}
		#endregion
	}
}