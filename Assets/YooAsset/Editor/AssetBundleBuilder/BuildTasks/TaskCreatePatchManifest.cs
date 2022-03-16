using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	/// <summary>
	/// 创建补丁清单文件
	/// </summary>
	public class TaskCreatePatchManifest : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var encryptionContext = context.GetContextObject<TaskEncryption.EncryptionContext>();
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();
			CreatePatchManifestFile(buildParameters, buildMapContext, encryptionContext);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(AssetBundleBuilder.BuildParametersContext buildParameters,
			TaskGetBuildMap.BuildMapContext buildMapContext, TaskEncryption.EncryptionContext encryptionContext)
		{
			// 创建新补丁清单
			PatchManifest patchManifest = new PatchManifest();
			patchManifest.ResourceVersion = buildParameters.Parameters.BuildVersion;
			patchManifest.BuildinTags = buildParameters.Parameters.BuildinTags;
			patchManifest.BundleList = GetAllPatchBundle(buildParameters, buildMapContext, encryptionContext);
			patchManifest.AssetList = GetAllPatchAsset(buildMapContext, patchManifest);

			// 创建补丁清单文件
			string manifestFilePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.PatchManifestFileName}";
			UnityEngine.Debug.Log($"创建补丁清单文件：{manifestFilePath}");
			PatchManifest.Serialize(manifestFilePath, patchManifest);

			// 创建补丁清单哈希文件
			string manifestHashFilePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.PatchManifestHashFileName}";
			string manifestHash = HashUtility.FileMD5(manifestFilePath);
			UnityEngine.Debug.Log($"创建补丁清单哈希文件：{manifestHashFilePath}");
			FileUtility.CreateFile(manifestHashFilePath, manifestHash);
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		private List<PatchBundle> GetAllPatchBundle(AssetBundleBuilder.BuildParametersContext buildParameters,
			TaskGetBuildMap.BuildMapContext buildMapContext, TaskEncryption.EncryptionContext encryptionContext)
		{
			List<PatchBundle> result = new List<PatchBundle>(1000);

			// 内置标记列表
			List<string> buildinTags = buildParameters.Parameters.GetBuildinTags();

			// 加载旧补丁清单
			PatchManifest oldPatchManifest = null;
			if (buildParameters.Parameters.IsForceRebuild == false)
			{
				oldPatchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory);
			}

			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var bundleName = bundleInfo.BundleName;
				string filePath = $"{buildParameters.PipelineOutputDirectory}/{bundleName}";
				string hash = HashUtility.FileMD5(filePath);
				string crc = HashUtility.FileCRC32(filePath);
				long size = FileUtility.GetFileSize(filePath);
				int version = buildParameters.Parameters.BuildVersion;
				string[] tags = buildMapContext.GetAssetTags(bundleName);
				bool isEncrypted = encryptionContext.IsEncryptFile(bundleName);
				bool isBuildin = IsBuildinBundle(tags, buildinTags);
				bool isRawFile = bundleInfo.IsRawFile;

				// 附加文件扩展名
				if (buildParameters.Parameters.AppendFileExtension)
				{
					hash += bundleInfo.GetAppendExtension();
				}

				// 注意：如果文件没有变化使用旧版本号
				if (oldPatchManifest != null && oldPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle value))
				{
					if (value.Hash == hash)
						version = value.Version;
				}

				PatchBundle patchBundle = new PatchBundle(bundleName, hash, crc, size, version, tags);
				patchBundle.SetFlagsValue(isEncrypted, isBuildin, isRawFile);
				result.Add(patchBundle);
			}

			return result;
		}
		private bool IsBuildinBundle(string[] bundleTags, List<string> buildinTags)
		{
			// 注意：没有任何标记的Bundle文件默认为内置文件
			if (bundleTags.Length == 0)
				return true;

			foreach (var tag in bundleTags)
			{
				if (buildinTags.Contains(tag))
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		private List<PatchAsset> GetAllPatchAsset(TaskGetBuildMap.BuildMapContext buildMapContext, PatchManifest patchManifest)
		{
			List<PatchAsset> result = new List<PatchAsset>(1000);
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var assetInfos = bundleInfo.GetCollectAssetInfos();
				foreach (var assetInfo in assetInfos)
				{
					PatchAsset patchAsset = new PatchAsset();
					patchAsset.AssetPath = assetInfo.AssetPath;
					patchAsset.BundleID = GetAssetBundleID(assetInfo.BundleName, patchManifest);
					patchAsset.DependIDs = GetAssetBundleDependIDs(assetInfo, patchManifest);
					result.Add(patchAsset);
				}
			}
			return result;
		}
		private int[] GetAssetBundleDependIDs(BuildAssetInfo assetInfo, PatchManifest patchManifest)
		{
			List<int> result = new List<int>();
			foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
			{
				if (dependAssetInfo.BundleNameIsValid() == false)
					continue;
				int bundleID = GetAssetBundleID(dependAssetInfo.BundleName, patchManifest);
				if (result.Contains(bundleID) == false)
					result.Add(bundleID);
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
	}
}