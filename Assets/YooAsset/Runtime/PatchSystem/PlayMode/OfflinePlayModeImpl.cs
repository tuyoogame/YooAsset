using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class OfflinePlayModeImpl : IBundleServices
	{
		internal PatchManifest AppPatchManifest;
		
		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync()
		{
			var operation = new OfflinePlayModeInitializationOperation(this);
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
				return new BundleInfo(string.Empty, string.Empty);

			if (AppPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				string localPath = AssetPathHelper.MakeStreamingLoadPath(patchBundle.Hash);
				BundleInfo bundleInfo = new BundleInfo(patchBundle, localPath);
				return bundleInfo;
			}
			else
			{
				YooLogger.Warning($"Not found bundle in patch manifest : {bundleName}");
				BundleInfo bundleInfo = new BundleInfo(bundleName, string.Empty);
				return bundleInfo;
			}
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