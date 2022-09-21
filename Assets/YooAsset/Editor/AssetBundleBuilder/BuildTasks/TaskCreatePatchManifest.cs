using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
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
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			int resourceVersion = buildParameters.Parameters.BuildVersion;

			// 创建新补丁清单
			PatchManifest patchManifest = new PatchManifest();
			patchManifest.FileVersion = YooAssetSettings.PatchManifestFileVersion;
			patchManifest.ResourceVersion = buildParameters.Parameters.BuildVersion;
			patchManifest.EnableAddressable = buildParameters.Parameters.EnableAddressable;
			patchManifest.OutputNameStyle = (int)buildParameters.Parameters.OutputNameStyle;
			patchManifest.BuildinTags = buildParameters.Parameters.BuildinTags;
			patchManifest.BundleList = GetAllPatchBundle(context);
			patchManifest.AssetList = GetAllPatchAsset(context, patchManifest);

			// 更新Unity内置资源包的引用关系
			if (buildParameters.Parameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if(buildParameters.Parameters.BuildMode == EBuildMode.IncrementalBuild)
				{
					var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
					UpdateBuiltInBundleReference(patchManifest, buildResultContext.Results);
				}
			}

			// 创建补丁清单文件
			string manifestFilePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(resourceVersion)}";
			BuildRunner.Log($"创建补丁清单文件：{manifestFilePath}");
			PatchManifest.Serialize(manifestFilePath, patchManifest);

			// 创建补丁清单哈希文件
			string manifestHashFilePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(resourceVersion)}";
			string manifestHash = HashUtility.FileMD5(manifestFilePath);
			BuildRunner.Log($"创建补丁清单哈希文件：{manifestHashFilePath}");
			FileUtility.CreateFile(manifestHashFilePath, manifestHash);

			// 创建静态版本文件
			string staticVersionFilePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.VersionFileName}";
			string staticVersion = resourceVersion.ToString();
			BuildRunner.Log($"创建静态版本文件：{staticVersionFilePath}");
			FileUtility.CreateFile(staticVersionFilePath, staticVersion);
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		private List<PatchBundle> GetAllPatchBundle(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var encryptionContext = context.GetContextObject<TaskEncryption.EncryptionContext>();

			List<PatchBundle> result = new List<PatchBundle>(1000);

			List<string> buildinTags = buildParameters.Parameters.GetBuildinTags();
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var bundleName = bundleInfo.BundleName;
				string fileHash = GetBundleFileHash(bundleInfo, buildParameters);
				string fileCRC = GetBundleFileCRC(bundleInfo, buildParameters);
				long fileSize = GetBundleFileSize(bundleInfo, buildParameters);
				string[] tags = buildMapContext.GetBundleTags(bundleName);
				bool isEncrypted = encryptionContext.IsEncryptFile(bundleName);
				bool isBuildin = IsBuildinBundle(tags, buildinTags);
				bool isRawFile = bundleInfo.IsRawFile;

				PatchBundle patchBundle = new PatchBundle(bundleName, fileHash, fileCRC, fileSize, tags);
				patchBundle.SetFlagsValue(isEncrypted, isBuildin, isRawFile);
				result.Add(patchBundle);
			}

			return result;
		}
		private bool IsBuildinBundle(string[] bundleTags, List<string> buildinTags)
		{
			// 注意：没有任何分类标签的Bundle文件默认为内置文件
			if (bundleTags.Length == 0)
				return true;

			foreach (var tag in bundleTags)
			{
				if (buildinTags.Contains(tag))
					return true;
			}
			return false;
		}
		private string GetBundleFileHash(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000000000000000000000000000"; //32位

			string filePath = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
			return HashUtility.FileMD5(filePath);
		}
		private string GetBundleFileCRC(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000"; //8位

			string filePath = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
			return HashUtility.FileCRC32(filePath);
		}
		private long GetBundleFileSize(BuildBundleInfo bundleInfo, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return 0;

			string filePath = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
			return FileUtility.GetFileSize(filePath);
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		private List<PatchAsset> GetAllPatchAsset(BuildContext context, PatchManifest patchManifest)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			List<PatchAsset> result = new List<PatchAsset>(1000);
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var assetInfos = bundleInfo.GetAllPatchAssetInfos();
				foreach (var assetInfo in assetInfos)
				{
					PatchAsset patchAsset = new PatchAsset();
					if (buildParameters.Parameters.EnableAddressable)
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