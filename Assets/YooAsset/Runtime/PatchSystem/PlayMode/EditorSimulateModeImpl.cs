using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class EditorSimulateModeImpl : IPlayModeServices, IBundleServices
	{
		private PatchManifest _activePatchManifest;
		private string _packageName;
		private bool _locationToLower;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(string packageName, bool locationToLower, string simulatePatchManifestPath)
		{
			_packageName = packageName;
			_locationToLower = locationToLower;
			var operation = new EditorSimulateModeInitializationOperation(this, simulatePatchManifestPath);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		#region IPlayModeServices接口
		public PatchManifest ActivePatchManifest
		{
			set
			{
				_activePatchManifest = value;
				_activePatchManifest.InitAssetPathMapping(_locationToLower);
			}
			get
			{
				return _activePatchManifest;
			}
		}
		public string GetPackageVersion()
		{
			if (_activePatchManifest == null)
				return string.Empty;
			return _activePatchManifest.PackageVersion;
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
		UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, bool autoSaveManifestFile, int timeout)
		{
			var operation = new EditorPlayModeUpdatePackageManifestOperation();
			OperationSystem.StartOperation(operation);
			return operation;
		}
		CheckContentsIntegrityOperation IPlayModeServices.CheckContentsIntegrityAsync()
		{
			var operation = new EditorSimulateModeCheckContentsIntegrityOperation();
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
			var patchBundle = _activePatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromEditor, assetInfo.AssetPath);
			return bundleInfo;
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			throw new NotImplementedException();
		}
		AssetInfo[] IBundleServices.GetAssetInfos(string[] tags)
		{
			return _activePatchManifest.GetAssetsInfoByTags(tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (_activePatchManifest.TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return _activePatchManifest.MappingToAssetPath(location);
		}
		string IBundleServices.TryMappingToAssetPath(string location)
		{
			return _activePatchManifest.TryMappingToAssetPath(location);
		}
		string IBundleServices.GetPackageName()
		{
			return _packageName;
		}
		bool IBundleServices.IsIncludeBundleFile(string fileName)
		{
			return _activePatchManifest.IsIncludeBundleFile(fileName);
		}
		bool IBundleServices.IsServicesValid()
		{
			return _activePatchManifest != null;
		}
		#endregion
	}
}