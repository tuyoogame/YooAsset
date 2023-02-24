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
					UpdateBuiltInBundleReference(patchManifest, buildResultContext, buildMapContext.ShadersBundleName);
				}
			}

			// 更新资源包之间的引用关系
			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
					UpdateScriptPipelineReference(patchManifest, buildResultContext);
				}
			}

			// 更新资源包之间的引用关系
			if (buildParameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding.BuildResultContext>();
					UpdateBuiltinPipelineReference(patchManifest, buildResultContext, buildMapContext);
				}
			}

			// 创建补丁清单文本文件
			{
				string fileName = YooAssetSettingsData.GetManifestJsonFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				PatchManifestTools.SerializeToJson(filePath, patchManifest);
				BuildRunner.Log($"创建补丁清单文件：{filePath}");
			}

			// 创建补丁清单二进制文件
			string packageHash;
			{
				string fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				PatchManifestTools.SerializeToBinary(filePath, patchManifest);
				packageHash = HashUtility.FileMD5(filePath);
				BuildRunner.Log($"创建补丁清单文件：{filePath}");

				PatchManifestContext patchManifestContext = new PatchManifestContext();
				byte[] bytesData = FileUtility.ReadAllBytes(filePath);
				patchManifestContext.Manifest = PatchManifestTools.DeserializeFromBinary(bytesData);
				context.SetContextObject(patchManifestContext);
			}

			// 创建补丁清单哈希文件
			{
				string fileName = YooAssetSettingsData.GetPackageHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string filePath = $"{packageOutputDirectory}/{fileName}";
				FileUtility.CreateFile(filePath, packageHash);
				BuildRunner.Log($"创建补丁清单哈希文件：{filePath}");
			}

			// 创建补丁清单版本文件
			{
				string fileName = YooAssetSettingsData.GetPackageVersionFileName(buildParameters.PackageName);
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
					patchAsset.BundleID = GetAssetBundleID(assetInfo.BundleName, patchManifest);
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
					int bundleID = GetAssetBundleID(dependAssetInfo.BundleName, patchManifest);
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
		private void UpdateBuiltInBundleReference(PatchManifest patchManifest, TaskBuilding_SBP.BuildResultContext buildResultContext, string shadersBunldeName)
		{
			// 获取所有依赖着色器资源包的资源包列表
			List<string> shaderBundleReferenceList = new List<string>();
			foreach (var valuePair in buildResultContext.Results.BundleInfos)
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

		/// <summary>
		/// 更新资源包之间的引用关系
		/// </summary>
		private void UpdateScriptPipelineReference(PatchManifest patchManifest, TaskBuilding_SBP.BuildResultContext buildResultContext)
		{
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchBundle.ReferenceIDs = GetScriptPipelineRefrenceIDs(patchManifest, patchBundle, buildResultContext);
			}
		}
		private int[] GetScriptPipelineRefrenceIDs(PatchManifest patchManifest, PatchBundle patchBundle, TaskBuilding_SBP.BuildResultContext buildResultContext)
		{
			if (buildResultContext.Results.BundleInfos.TryGetValue(patchBundle.BundleName, out var details) == false)
			{
				throw new Exception("Should never get here !");
			}

			List<string> referenceList = new List<string>();
			foreach (var keyValuePair in buildResultContext.Results.BundleInfos)
			{
				string bundleName = keyValuePair.Key;
				if (bundleName == patchBundle.BundleName)
					continue;
				if (keyValuePair.Value.Dependencies.Contains(patchBundle.BundleName))
				{
					referenceList.Add(bundleName);
				}
			}

			List<int> result = new List<int>();
			foreach (var bundleName in referenceList)
			{
				int bundleID = GetAssetBundleID(bundleName, patchManifest);
				if (result.Contains(bundleID) == false)
					result.Add(bundleID);
			}
			return result.ToArray();
		}

		/// <summary>
		/// 更新资源包之间的引用关系
		/// </summary>
		private void UpdateBuiltinPipelineReference(PatchManifest patchManifest, TaskBuilding.BuildResultContext buildResultContext, BuildMapContext buildMapContext)
		{
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchBundle.ReferenceIDs = GetBuiltinPipelineRefrenceIDs(patchManifest, patchBundle, buildResultContext, buildMapContext);
			}
		}
		private int[] GetBuiltinPipelineRefrenceIDs(PatchManifest patchManifest, PatchBundle patchBundle, TaskBuilding.BuildResultContext buildResultContext, BuildMapContext buildMapContext)
		{
			List<string> referenceList = new List<string>();
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				string bundleName = bundleInfo.BundleName;
				if (bundleName == patchBundle.BundleName)
					continue;
				string[] dependencies = buildResultContext.UnityManifest.GetDirectDependencies(bundleName);
				if (dependencies.Contains(patchBundle.BundleName))
				{
					referenceList.Add(bundleName);
				}
			}

			List<int> result = new List<int>();
			foreach (var bundleName in referenceList)
			{
				int bundleID = GetAssetBundleID(bundleName, patchManifest);
				if (result.Contains(bundleID) == false)
					result.Add(bundleID);
			}
			return result.ToArray();
		}
	}
}