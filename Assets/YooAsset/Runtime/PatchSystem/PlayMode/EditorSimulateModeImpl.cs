using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class EditorSimulateModeImpl : IBundleServices
	{
		private PatchManifest _simulatePatchManifest;
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

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (_simulatePatchManifest == null)
				return 0;
			return _simulatePatchManifest.ResourceVersion;
		}

		// 设置资源清单
		internal void SetSimulatePatchManifest(PatchManifest patchManifest)
		{
			_simulatePatchManifest = patchManifest;
			_simulatePatchManifest.InitAssetPathMapping(_locationToLower);
		}

		#region IBundleServices接口
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var patchBundle = _simulatePatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromEditor, assetInfo.AssetPath);
			return bundleInfo;
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			throw new NotImplementedException();
		}
		AssetInfo[] IBundleServices.GetAssetInfos(string[] tags)
		{
			return PatchHelper.GetAssetsInfoByTags(_simulatePatchManifest, tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (_simulatePatchManifest.TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return _simulatePatchManifest.MappingToAssetPath(location);
		}
		#endregion
	}
}