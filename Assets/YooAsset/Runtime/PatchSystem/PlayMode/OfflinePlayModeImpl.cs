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
			OperationSystem.StartOperation(operation);
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

		// 设置资源清单
		internal void SetAppPatchManifest(PatchManifest patchManifest)
		{
			_appPatchManifest = patchManifest;
			_appPatchManifest.InitAssetPathMapping(_locationToLower);
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
			var patchBundle = _appPatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
			return CreateBundleInfo(patchBundle);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var depends = _appPatchManifest.GetAllDependencies(assetInfo.AssetPath);
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
			return PatchHelper.GetAssetsInfoByTags(_appPatchManifest, tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (_appPatchManifest.TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return _appPatchManifest.MappingToAssetPath(location);
		}
		#endregion
	}
}