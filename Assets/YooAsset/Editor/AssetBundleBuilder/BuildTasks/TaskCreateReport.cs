using System;
using System.Collections.Generic;
using System.IO;

namespace YooAsset.Editor
{
	/// <summary>
	/// 创建报告文件
	/// </summary>
	public class TaskCreateReport : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			CreateReportFile(buildParameters, buildMapContext);
		}

		private void CreateReportFile(AssetBundleBuilder.BuildParametersContext buildParameters, BuildMapContext buildMapContext)
		{
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory, buildParameters.Parameters.BuildVersion);
			BuildReport buildReport = new BuildReport();
			buildParameters.StopWatch();

			// 概述信息
			{
				buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
				buildReport.Summary.BuildTime = DateTime.Now.ToString();
				buildReport.Summary.BuildSeconds = buildParameters.GetBuildingSeconds();
				buildReport.Summary.BuildTarget = buildParameters.Parameters.BuildTarget;
				buildReport.Summary.BuildVersion = buildParameters.Parameters.BuildVersion;
				buildReport.Summary.EnableAutoCollect = buildParameters.Parameters.EnableAutoCollect;
				buildReport.Summary.AppendFileExtension = buildParameters.Parameters.AppendFileExtension;
				buildReport.Summary.AutoCollectShaders = AssetBundleGrouperSettingData.Setting.AutoCollectShaders;
				buildReport.Summary.ShadersBundleName = AssetBundleGrouperSettingData.Setting.ShadersBundleName;
				buildReport.Summary.EncryptionServicesClassName = buildParameters.Parameters.EncryptionServices == null ?
					"null" : buildParameters.Parameters.EncryptionServices.GetType().FullName;
				
				// 构建参数
				buildReport.Summary.ForceRebuild = buildParameters.Parameters.ForceRebuild;
				buildReport.Summary.BuildinTags = buildParameters.Parameters.BuildinTags;
				buildReport.Summary.CompressOption = buildParameters.Parameters.CompressOption;
				buildReport.Summary.AppendHash = buildParameters.Parameters.AppendHash;
				buildReport.Summary.DisableWriteTypeTree = buildParameters.Parameters.DisableWriteTypeTree;
				buildReport.Summary.IgnoreTypeTreeChanges = buildParameters.Parameters.IgnoreTypeTreeChanges;
				buildReport.Summary.DisableLoadAssetByFileName = buildParameters.Parameters.DisableLoadAssetByFileName;

				// 构建结果
				buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
				buildReport.Summary.AllBundleTotalCount = GetAllBundleCount(patchManifest);
				buildReport.Summary.AllBundleTotalSize = GetAllBundleSize(patchManifest);
				buildReport.Summary.BuildinBundleTotalCount = GetBuildinBundleCount(patchManifest);
				buildReport.Summary.BuildinBundleTotalSize = GetBuildinBundleSize(patchManifest);
				buildReport.Summary.EncryptedBundleTotalCount = GetEncryptedBundleCount(patchManifest);
				buildReport.Summary.EncryptedBundleTotalSize = GetEncryptedBundleSize(patchManifest);
				buildReport.Summary.RawBundleTotalCount = GetRawBundleCount(patchManifest);
				buildReport.Summary.RawBundleTotalSize = GetRawBundleSize(patchManifest);
			}

			// 资源对象列表
			buildReport.AssetInfos = new List<ReportAssetInfo>(patchManifest.AssetList.Count);
			foreach (var patchAsset in patchManifest.AssetList)
			{
				var mainBundle = patchManifest.BundleList[patchAsset.BundleID];
				ReportAssetInfo reportAssetInfo = new ReportAssetInfo();
				reportAssetInfo.AssetPath = patchAsset.AssetPath;
				reportAssetInfo.MainBundle = mainBundle.BundleName;
				reportAssetInfo.DependBundles = GetDependBundles(patchManifest, patchAsset);
				reportAssetInfo.DependAssets = GetDependAssets(buildMapContext, mainBundle.BundleName, patchAsset.AssetPath);
				buildReport.AssetInfos.Add(reportAssetInfo);
			}

			// 资源包列表
			buildReport.BundleInfos = new List<ReportBundleInfo>(patchManifest.BundleList.Count);
			foreach (var patchBundle in patchManifest.BundleList)
			{
				ReportBundleInfo reportBundleInfo = new ReportBundleInfo();
				reportBundleInfo.BundleName = patchBundle.BundleName;
				reportBundleInfo.Hash = patchBundle.Hash;
				reportBundleInfo.CRC = patchBundle.CRC;
				reportBundleInfo.SizeBytes = patchBundle.SizeBytes;
				reportBundleInfo.Tags = patchBundle.Tags;
				reportBundleInfo.Flags = patchBundle.Flags;
				buildReport.BundleInfos.Add(reportBundleInfo);
			}

			// 删除旧文件
			string filePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.ReportFileName}";
			if (File.Exists(filePath))
				File.Delete(filePath);

			// 序列化文件
			BuildReport.Serialize(filePath, buildReport);
		}

		/// <summary>
		/// 获取资源对象依赖的所有资源包
		/// </summary>
		private List<string> GetDependBundles(PatchManifest patchManifest, PatchAsset patchAsset)
		{
			List<string> dependBundles = new List<string>(patchAsset.DependIDs.Length);
			foreach (int index in patchAsset.DependIDs)
			{
				string dependBundleName = patchManifest.BundleList[index].BundleName;
				dependBundles.Add(dependBundleName);
			}
			return dependBundles;
		}

		/// <summary>
		/// 获取资源对象依赖的其它所有资源
		/// </summary>
		private List<string> GetDependAssets(BuildMapContext buildMapContext, string bundleName, string assetPath)
		{
			List<string> result = new List<string>();
			if (buildMapContext.TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
			{
				BuildAssetInfo findAssetInfo = null;
				foreach (var buildinAsset in bundleInfo.BuildinAssets)
				{
					if (buildinAsset.AssetPath == assetPath)
					{
						findAssetInfo = buildinAsset;
						break;
					}
				}
				if (findAssetInfo == null)
				{
					throw new Exception($"Not found asset {assetPath} in bunlde {bundleName}");
				}
				foreach (var dependAssetInfo in findAssetInfo.AllDependAssetInfos)
				{
					result.Add(dependAssetInfo.AssetPath);
				}
			}
			else
			{
				throw new Exception($"Not found bundle : {bundleName}");
			}
			return result;
		}

		private int GetAllBundleCount(PatchManifest patchManifest)
		{
			return patchManifest.BundleList.Count;
		}
		private long GetAllBundleSize(PatchManifest patchManifest)
		{
			long fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				fileBytes += patchBundle.SizeBytes;
			}
			return fileBytes;
		}
		private int GetBuildinBundleCount(PatchManifest patchManifest)
		{
			int fileCount = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin)
					fileCount++;
			}
			return fileCount;
		}
		private long GetBuildinBundleSize(PatchManifest patchManifest)
		{
			long fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin)
					fileBytes += patchBundle.SizeBytes;
			}
			return fileBytes;
		}
		private int GetEncryptedBundleCount(PatchManifest patchManifest)
		{
			int fileCount = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsEncrypted)
					fileCount++;
			}
			return fileCount;
		}
		private long GetEncryptedBundleSize(PatchManifest patchManifest)
		{
			long fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsEncrypted)
					fileBytes += patchBundle.SizeBytes;
			}
			return fileBytes;
		}
		private int GetRawBundleCount(PatchManifest patchManifest)
		{
			int fileCount = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsRawFile)
					fileCount++;
			}
			return fileCount;
		}
		private long GetRawBundleSize(PatchManifest patchManifest)
		{
			long fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsRawFile)
					fileBytes += patchBundle.SizeBytes;
			}
			return fileBytes;
		}
	}
}