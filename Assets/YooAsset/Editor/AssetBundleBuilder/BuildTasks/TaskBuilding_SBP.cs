using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建内容打包")]
	public class TaskBuilding_SBP : IBuildTask
	{
		public class BuildResultContext : IContextObject
		{
			public IBundleBuildResults Results;
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			// 模拟构建模式下跳过引擎构建
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return;

			// 构建内容
			var buildContent = new BundleBuildContent(buildMapContext.GetPipelineBuilds());

			// 开始构建
			IBundleBuildResults buildResults;
			var buildParameters = buildParametersContext.GetSBPBuildParameters();
			var shadersBunldeName = YooAssetSettingsData.GetUnityShadersBundleFullName();
			var taskList = SBPBuildTasks.Create(shadersBunldeName);
			ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
			if (exitCode < 0)
			{
				throw new Exception($"构建过程中发生错误 : {exitCode}");
			}

			BuildRunner.Log("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new BuildResultContext();
			buildResultContext.Results = buildResults;
			context.SetContextObject(buildResultContext);

			// 拷贝原生文件
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyRawBundle(buildMapContext, buildParametersContext);
			}
		}

		/// <summary>
		/// 拷贝原生文件
		/// </summary>
		private void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
		{
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
					foreach (var buildAsset in bundleInfo.BuildinAssets)
					{
						if (buildAsset.IsRawAsset)
							EditorTools.CopyFile(buildAsset.AssetPath, dest, true);
					}
				}
			}
		}
	}
}