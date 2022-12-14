using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	internal class OfflinePlayModeImpl : IBundleServices
	{
		public PatchManifest AppPatchManifest { private set; get; }
		private bool _locationToLower;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool locationToLower, string packageName)
		{
			_locationToLower = locationToLower;
			var operation = new OfflinePlayModeInitializationOperation(this, packageName);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 获取包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			if (AppPatchManifest == null)
				return string.Empty;
			return AppPatchManifest.PackageVersion;
		}

		internal void SetAppPatchManifest(PatchManifest patchManifest)
		{
			AppPatchManifest = patchManifest;
			AppPatchManifest.InitAssetPathMapping(_locationToLower);
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
			{
				BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}
		}
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var patchBundle = AppPatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
			return CreateBundleInfo(patchBundle);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var depends = AppPatchManifest.GetAllDependencies(assetInfo.AssetPath);
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
			return AppPatchManifest.GetAssetsInfoByTags(tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (AppPatchManifest.TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return AppPatchManifest.MappingToAssetPath(location);
		}
		string IBundleServices.TryMappingToAssetPath(string location)
		{
			return AppPatchManifest.TryMappingToAssetPath(location);
		}
		string IBundleServices.GetPackageName()
		{
			return AppPatchManifest.PackageName;
		}
		bool IBundleServices.IsIncludeBundleFile(string fileName)
		{
			return AppPatchManifest.IsIncludeBundleFile(fileName);
		}
		bool IBundleServices.IsServicesValid()
		{
			return AppPatchManifest != null;
		}
		#endregion
	}
}