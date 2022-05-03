using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class EditorPlayModeImpl : IBundleServices
	{
		internal PatchManifest AppPatchManifest;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync()
		{
			var operation = new EditorPlayModeInitializationOperation(this);
			OperationSystem.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (AppPatchManifest == null)
				return 0;
			return AppPatchManifest.ResourceVersion;
		}

		#region IBundleServices接口
		BundleInfo IBundleServices.GetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new BundleInfo(string.Empty);

			if (AppPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				string mainAssetPath = AppPatchManifest.TryGetBundleMainAssetPath(bundleName);
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
			return PatchHelper.GetAssetsInfoByTag(AppPatchManifest, tags);
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return AppPatchManifest.MappingToAssetPath(location);
		}
		string IBundleServices.GetBundleName(string assetPath)
		{
			return AppPatchManifest.GetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return AppPatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}