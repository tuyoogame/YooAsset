using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class EditorSimulateModeImpl : IBundleServices
	{
		public PatchManifest ActivePatchManifest { private set; get; }
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
		/// 获取包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			if (ActivePatchManifest == null)
				return string.Empty;
			return ActivePatchManifest.PackageVersion;
		}

		internal void SetActivePatchManifest(PatchManifest patchManifest)
		{
			ActivePatchManifest = patchManifest;
			ActivePatchManifest.InitAssetPathMapping(_locationToLower);
		}

		#region IBundleServices接口
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			var patchBundle = ActivePatchManifest.GetMainPatchBundle(assetInfo.AssetPath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromEditor, assetInfo.AssetPath);
			return bundleInfo;
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			throw new NotImplementedException();
		}
		AssetInfo[] IBundleServices.GetAssetInfos(string[] tags)
		{
			return ActivePatchManifest.GetAssetsInfoByTags(tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (ActivePatchManifest.TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return ActivePatchManifest.MappingToAssetPath(location);
		}
		string IBundleServices.TryMappingToAssetPath(string location)
		{
			return ActivePatchManifest.TryMappingToAssetPath(location);
		}
		string IBundleServices.GetPackageName()
		{
			return ActivePatchManifest.PackageName;
		}
		bool IBundleServices.IsIncludeBundleFile(string fileName)
		{
			return ActivePatchManifest.IsIncludeBundleFile(fileName);
		}
		bool IBundleServices.IsServicesValid()
		{
			return ActivePatchManifest != null;
		}
		#endregion
	}
}