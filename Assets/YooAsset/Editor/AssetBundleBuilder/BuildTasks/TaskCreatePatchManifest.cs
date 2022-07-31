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
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var encryptionContext = context.GetContextObject<TaskEncryption.EncryptionContext>();
			int resourceVersion = buildParameters.Parameters.BuildVersion;

			// 创建新补丁清单
			PatchManifest patchManifest = new PatchManifest();
			patchManifest.FileVersion = YooAssetSettings.PatchManifestFileVersion;
			patchManifest.ResourceVersion = buildParameters.Parameters.BuildVersion;
			patchManifest.EnableAddressable = buildParameters.Parameters.EnableAddressable;
			patchManifest.OutputNameStyle = (int)buildParameters.Parameters.OutputNameStyle;
			patchManifest.BuildinTags = buildParameters.Parameters.BuildinTags;
			patchManifest.BundleList = GetAllPatchBundle(buildParameters, buildMapContext, encryptionContext);
			patchManifest.AssetList = GetAllPatchAsset(buildParameters, buildMapContext, patchManifest);

			// 更新Unity内置资源包的引用关系
			if (buildParameters.Parameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				var buildResultContext = context.GetContextObject<TaskBuilding_SBP.SBPBuildResultContext>();
				UpdateBuiltInBundleReference(patchManifest, buildResultContext.Results);
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
		private List<PatchBundle> GetAllPatchBundle(BuildParametersContext buildParameters, BuildMapContext buildMapContext,
			TaskEncryption.EncryptionContext encryptionContext)
		{
			List<PatchBundle> result = new List<PatchBundle>(1000);

			List<string> buildinTags = buildParameters.Parameters.GetBuildinTags();
			var buildMode = buildParameters.Parameters.BuildMode;
			bool standardBuild = buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild;
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var bundleName = bundleInfo.BundleName;
				string filePath = $"{buildParameters.PipelineOutputDirectory}/{bundleName}";
				string hash = GetFileHash(filePath, standardBuild);
				string crc32 = GetFileCRC(filePath, standardBuild);
				long size = GetFileSize(filePath, standardBuild);
				string[] tags = buildMapContext.GetBundleTags(bundleName);
				bool isEncrypted = encryptionContext.IsEncryptFile(bundleName);
				bool isBuildin = IsBuildinBundle(tags, buildinTags);
				bool isRawFile = bundleInfo.IsRawFile;

				PatchBundle patchBundle = new PatchBundle(bundleName, hash, crc32, size, tags);
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
		private string GetFileHash(string filePath, bool standardBuild)
		{
			if (standardBuild)
				return HashUtility.FileMD5(filePath);
			else
				return "00000000000000000000000000000000"; //32位
		}
		private string GetFileCRC(string filePath, bool standardBuild)
		{
			if (standardBuild)
				return HashUtility.FileCRC32(filePath);
			else
				return "00000000"; //8位
		}
		private long GetFileSize(string filePath, bool standardBuild)
		{
			if (standardBuild)
				return FileUtility.GetFileSize(filePath);
			else
				return 0;
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		private List<PatchAsset> GetAllPatchAsset(BuildParametersContext buildParameters, BuildMapContext buildMapContext, PatchManifest patchManifest)
		{
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
			// 获取所有依赖内置资源包的资源包列表
			List<string> builtInBundleReferenceList = new List<string>();
			foreach (var valuePair in buildResults.BundleInfos)
			{
				if (valuePair.Value.Dependencies.Any(t => t == YooAssetSettings.UnityBuiltInShadersBundleName))
					builtInBundleReferenceList.Add(valuePair.Key);
			}

			// 检测依赖交集并更新依赖ID
			int builtInBundleId = patchManifest.BundleList.Count - 1;
			foreach (var patchAsset in patchManifest.AssetList)
			{
				List<string> dependBundles = GetPatchAssetAllDependBundles(patchManifest, patchAsset);
				List<string> conflictAssetPathList = dependBundles.Intersect(builtInBundleReferenceList).ToList();
				if (conflictAssetPathList.Count > 0)
				{
					List<int> newDependIDs = new List<int>(patchAsset.DependIDs);
					newDependIDs.Add(builtInBundleId);
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