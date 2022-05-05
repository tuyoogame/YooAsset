using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class OfflinePlayModeImpl : IBundleServices
	{
		private PatchManifest _appPatchManifest;
		private bool _locationToLower;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool locationToLower)
		{
			_locationToLower = locationToLower;
			var operation = new OfflinePlayModeInitializationOperation(this);
			OperationSystem.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (_appPatchManifest == null)
				return 0;
			return _appPatchManifest.ResourceVersion;
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByTags(string[] tags, int fileUpackingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> unpcakList = PatchHelper.GetUnpackListByTags(_appPatchManifest, tags);
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain);
			return operation;
		}

		// 设置资源清单
		internal void SetAppPatchManifest(PatchManifest patchManifest)
		{
			_appPatchManifest = patchManifest;
			_appPatchManifest.InitAssetPathMapping(_locationToLower);
		}

		#region IBundleServices接口
		BundleInfo IBundleServices.GetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new BundleInfo(string.Empty);

			if (_appPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}
			else
			{
				YooLogger.Warning($"Not found bundle in patch manifest : {bundleName}");
				BundleInfo bundleInfo = new BundleInfo(bundleName);
				return bundleInfo;
			}
		}
		AssetInfo[] IBundleServices.GetAssetInfos(string[] tags)
		{
			return PatchHelper.GetAssetsInfoByTag(_appPatchManifest, tags);
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return _appPatchManifest.MappingToAssetPath(location);
		}
		string IBundleServices.GetBundleName(string assetPath)
		{
			return _appPatchManifest.GetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return _appPatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}