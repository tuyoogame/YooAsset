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
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();
			CreateReportFile(buildParameters, buildMapContext);
		}

		private void CreateReportFile(AssetBundleBuilder.BuildParametersContext buildParameters, TaskGetBuildMap.BuildMapContext buildMapContext)
		{
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory);
			BuildReport buildReport = new BuildReport();

			// 概述信息
			buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
			buildReport.Summary.BuildTime = DateTime.Now.ToString();
			buildReport.Summary.BuildSeconds = 0;
			buildReport.Summary.BuildTarget = buildParameters.Parameters.BuildTarget;
			buildReport.Summary.BuildVersion = buildParameters.Parameters.BuildVersion;
			buildReport.Summary.ApplyRedundancy = buildParameters.Parameters.ApplyRedundancy;
			buildReport.Summary.AppendFileExtension = buildParameters.Parameters.AppendFileExtension;
			buildReport.Summary.IsCollectAllShaders = AssetBundleCollectorSettingData.Setting.IsCollectAllShaders;
			buildReport.Summary.ShadersBundleName = AssetBundleCollectorSettingData.Setting.ShadersBundleName;
			buildReport.Summary.IsForceRebuild = buildParameters.Parameters.IsForceRebuild;
			buildReport.Summary.BuildinTags = buildParameters.Parameters.BuildinTags;
			buildReport.Summary.CompressOption = buildParameters.Parameters.CompressOption;
			buildReport.Summary.IsAppendHash = buildParameters.Parameters.IsAppendHash;
			buildReport.Summary.IsDisableWriteTypeTree = buildParameters.Parameters.IsDisableWriteTypeTree;
			buildReport.Summary.IsIgnoreTypeTreeChanges = buildParameters.Parameters.IsIgnoreTypeTreeChanges;
			buildReport.Summary.IsDisableLoadAssetByFileName = buildParameters.Parameters.IsDisableLoadAssetByFileName;

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
				reportBundleInfo.Version = patchBundle.Version;
				reportBundleInfo.Tags = patchBundle.Tags;
				reportBundleInfo.Flags = patchBundle.Flags;
				buildReport.BundleInfos.Add(reportBundleInfo);
			}

			// 冗余资源列表
			buildReport.RedundancyAssetList = buildMapContext.RedundancyAssetList;

			// 删除旧文件
			string filePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.ReportFileName}";
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
			foreach(int index in patchAsset.DependIDs)
			{
				string dependBundleName = patchManifest.BundleList[index].BundleName;
				dependBundles.Add(dependBundleName);
			}
			return dependBundles;
		}

		/// <summary>
		/// 获取资源对象依赖的其它所有资源
		/// </summary>
		private List<string> GetDependAssets(TaskGetBuildMap.BuildMapContext buildMapContext, string bundleName, string assetPath)
		{
			List<string> result = new List<string>();
			if(buildMapContext.TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
			{
				BuildAssetInfo findAssetInfo = null;
				foreach(var buildinAsset in bundleInfo.BuildinAssets)
				{
					if(buildinAsset.AssetPath == assetPath)
					{
						findAssetInfo = buildinAsset;
						break;
					}
				}
				if (findAssetInfo == null)
				{
					throw new Exception($"Not found asset {assetPath} in bunlde {bundleName}");
				}
				foreach(var dependAssetInfo in findAssetInfo.AllDependAssetInfos)
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
	}
}