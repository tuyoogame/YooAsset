using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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
			BuildReport buildReport = new BuildReport();

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

			//buildReport.BundleInfos = buildMapContext.BundleInfos;
			buildReport.RedundancyList = buildMapContext.RedundancyList;

			// 删除旧文件
			string filePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.ReportFileName}";
			if (File.Exists(filePath))
				File.Delete(filePath);
			BuildReport.Serialize(filePath, buildReport);
		}
	}
}