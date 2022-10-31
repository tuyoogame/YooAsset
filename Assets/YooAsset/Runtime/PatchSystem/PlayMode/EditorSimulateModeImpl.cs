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
		/// 获取包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			if (_simulatePatchManifest == null)
				return string.Empty;
			return _simulatePatchManifest.PackageVersion;
		}

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
			return _simulatePatchManifest.GetAssetsInfoByTags(tags);
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
		string IBundleServices.TryMappingToAssetPath(string location)
		{
			return _simulatePatchManifest.TryMappingToAssetPath(location);
		}
		string IBundleServices.GetPackageName()
		{
			return _simulatePatchManifest.PackageName;
		}
		bool IBundleServices.IsIncludeBundleFile(string fileName)
		{
			return _simulatePatchManifest.IsIncludeBundleFile(fileName);
		}
		#endregion
	}
}