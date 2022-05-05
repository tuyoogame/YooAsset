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
		public InitializationOperation InitializeAsync(bool locationToLower)
		{
			_locationToLower = locationToLower;
			var operation = new EditorSimulateModeInitializationOperation(this);
			OperationSystem.ProcessOperaiton(operation);
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
		BundleInfo IBundleServices.GetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new BundleInfo(string.Empty);

			if (_simulatePatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				string mainAssetPath = _simulatePatchManifest.TryGetBundleMainAssetPath(bundleName);
				BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromEditor, mainAssetPath);
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
			return PatchHelper.GetAssetsInfoByTag(_simulatePatchManifest, tags);
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return _simulatePatchManifest.MappingToAssetPath(location);
		}
		string IBundleServices.GetBundleName(string assetPath)
		{
			return _simulatePatchManifest.GetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return _simulatePatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}