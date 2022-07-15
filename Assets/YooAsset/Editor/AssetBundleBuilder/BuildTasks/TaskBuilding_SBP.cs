using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建内容打包")]
	public class TaskBuilding_SBP : IBuildTask
	{
		public class SBPBuildResultContext : IContextObject
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
			var taskList = DefaultBuildTasks.Create(DefaultBuildTasks.Preset.AssetBundleBuiltInShaderExtraction);
			ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
			if (exitCode < 0)
			{
				throw new Exception($"构建过程中发生错误 : {exitCode}");
			}

			BuildRunner.Log("Unity引擎打包成功！");
			SBPBuildResultContext buildResultContext = new SBPBuildResultContext();
			buildResultContext.Results = buildResults;
			context.SetContextObject(buildResultContext);

			// 添加Unity内置资源包信息
			if (buildResults.BundleInfos.Keys.Any(t => t == YooAssetSettings.UnityBuiltInShadersBundleName))
			{
				BuildBundleInfo builtInBundleInfo = new BuildBundleInfo(YooAssetSettings.UnityBuiltInShadersBundleName);
				buildMapContext.BundleInfos.Add(builtInBundleInfo);
			}

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
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
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