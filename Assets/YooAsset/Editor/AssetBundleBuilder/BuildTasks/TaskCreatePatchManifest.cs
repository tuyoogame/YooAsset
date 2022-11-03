using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
	public class PatchManifestContext : IContextObject
	{
		internal PatchManifest Manifest;
	}

	[TaskAttribute("创建补丁清单文件")]
	public class TaskCreatePatchManifest : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			CreatePatchManifestFile(context);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(BuildContext context)
		{
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = buildParametersContext.Parameters;
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

			// 创建新补丁清单
			PatchManifest patchManifest = new PatchManifest();
			patchManifest.FileVersion = YooAssetSettings.PatchManifestFileVersion;
			patchManifest.EnableAddressable = buildMapContext.EnableAddressable;
			patchManifest.OutputNameStyle = (int)buildParameters.OutputNameStyle;
			patchManifest.PackageName = buildParameters.PackageName;
			patchManifest.PackageVersion = buildParameters.PackageVersion;
			patchManifest.BundleList = GetAllPatchBundle(context);
			patchManifest.AssetList = GetAllPatchAsset(context, patchManifest);

			// 更新Unity内置资源包的引用关系
			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
					UpdateBuiltInBundleReference(patchManifest, buildResultContext.Results);
				}
			}

			// 创建补丁清单文件
			string packageHash;
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				PatchManifest.Serialize(filePath, patchManifest);
				packageHash = HashUtility.FileMD5(filePath);
				BuildRunner.Log($"创建补丁清单文件：{filePath}");

				var patchManifestContext = new PatchManifestContext();
				string jsonData = FileUtility.ReadFile(filePath);
				patchManifestContext.Manifest = PatchManifest.Deserialize(jsonData);
				context.SetContextObject(patchManifestContext);
			}

			// 创建补丁清单哈希文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileUtility.CreateFile(filePath, packageHash);
				BuildRunner.Log($"创建补丁清单哈希文件：{filePath}");
			}

			// 创建补丁清单版本文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestVersionFileName(buildParameters.PackageName);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileUtility.CreateFile(filePath, buildParameters.PackageVersion);
				BuildRunner.Log($"创建补丁清单版本文件：{filePath}");
			}
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		private List<PatchBundle> GetAllPatchBundle(BuildContext context)
		{
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();

			List<PatchBundle> result = new List<PatchBundle>(1000);
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var patchBundle = bundleInfo.CreatePatchBundle();
				result.Add(patchBundle);
			}
			return result;
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		private List<PatchAsset> GetAllPatchAsset(BuildContext context, PatchManifest patchManifest)
		{
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			List<PatchAsset> result = new List<PatchAsset>(1000);
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var assetInfos = bundleInfo.GetAllPatchAssetInfos();
				foreach (var assetInfo in assetInfos)
				{
					PatchAsset patchAsset = new PatchAsset();
					if (buildMapContext.EnableAddressable)
						patchAsset.Address = assetInfo.Address;
					else
						patchAsset.Address = string.Empty;
					patchAsset.AssetPath = assetInfo.AssetPath;
					patchAsset.AssetTags = assetInfo.AssetTags.ToArray();
					patchAsset.BundleID = GetAssetBundleID(assetInfo.GetBundleName(), patchManifest);
					patchAsset.DependIDs = GetAssetBundleDependIDs(patchAsset.BundleID, assetInfo, patchManifest);
					result.Add(patchAsset);
				}
			}
			return result;
		}
		private int[] GetAssetBundleDependIDs(int mainBundleID, BuildAssetInfo assetInfo, PatchManifest patchManifest)
		{
			List<int> result = new List<int>();
			foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
			{
				if (dependAssetInfo.HasBundleName())
				{
					int bundleID = GetAssetBundleID(dependAssetInfo.GetBundleName(), patchManifest);
					if (mainBundleID != bundleID)
					{
						if (result.Contains(bundleID) == false)
							result.Add(bundleID);
					}
				}
			}
			return result.ToArray();
		}
		private int GetAssetBundleID(string bundleName, PatchManifest patchManifest)
		{
			for (int index = 0; index < patchManifest.BundleList.Count; index++)
			{
				if (patchManifest.BundleList[index].BundleName == bundleName)
					return index;
			}
			throw new Exception($"Not found bundle name : {bundleName}");
		}

		/// <summary>
		/// 更新Unity内置资源包的引用关系
		/// </summary>
		private void UpdateBuiltInBundleReference(PatchManifest patchManifest, IBundleBuildResults buildResults)
		{
			// 获取所有依赖着色器资源包的资源包列表
			string shadersBunldeName = YooAssetSettingsData.GetUnityShadersBundleFullName();
			List<string> shaderBundleReferenceList = new List<string>();
			foreach (var valuePair in buildResults.BundleInfos)
			{
				if (valuePair.Value.Dependencies.Any(t => t == shadersBunldeName))
					shaderBundleReferenceList.Add(valuePair.Key);
			}

			// 注意：没有任何资源依赖着色器
			if (shaderBundleReferenceList.Count == 0)
				return;

			// 获取着色器资源包索引
			Predicate<PatchBundle> predicate = new Predicate<PatchBundle>(s => s.BundleName == shadersBunldeName);
			int shaderBundleId = patchManifest.BundleList.FindIndex(predicate);
			if (shaderBundleId == -1)
				throw new Exception("没有发现着色器资源包！");

			// 检测依赖交集并更新依赖ID
			foreach (var patchAsset in patchManifest.AssetList)
			{
				List<string> dependBundles = GetPatchAssetAllDependBundles(patchManifest, patchAsset);
				List<string> conflictAssetPathList = dependBundles.Intersect(shaderBundleReferenceList).ToList();
				if (conflictAssetPathList.Count > 0)
				{
					List<int> newDependIDs = new List<int>(patchAsset.DependIDs);
					if (newDependIDs.Contains(shaderBundleId) == false)
						newDependIDs.Add(shaderBundleId);
					patchAsset.DependIDs = newDependIDs.ToArray();
				}
			}
		}
		private List<string> GetPatchAssetAllDependBundles(PatchManifest patchManifest, PatchAsset patchAsset)
		{
			List<string> result = new List<string>();
			string mainBundle = patchManifest.BundleList[patchAsset.BundleID].BundleName;
			result.Add(mainBundle);
			foreach (var dependID in patchAsset.DependIDs)
			{
				string dependBundle = patchManifest.BundleList[dependID].BundleName;
				result.Add(dependBundle);
			}
			return result;
		}
	}
}