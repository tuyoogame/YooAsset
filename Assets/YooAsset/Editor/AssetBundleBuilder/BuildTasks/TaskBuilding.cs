using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建内容打包")]
	public class TaskBuilding : IBuildTask
	{
		public class BuildResultContext : IContextObject
		{
			public AssetBundleManifest UnityManifest;
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			// 模拟构建模式下跳过引擎构建
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return;

			BuildAssetBundleOptions opt = buildParametersContext.GetPipelineBuildOptions();
			AssetBundleManifest buildResults = BuildPipeline.BuildAssetBundles(buildParametersContext.PipelineOutputDirectory, buildMapContext.GetPipelineBuilds(), opt, buildParametersContext.Parameters.BuildTarget);
			if (buildResults == null)
				throw new Exception("构建过程中发生错误！");

			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				string unityOutputManifestFilePath = $"{buildParametersContext.PipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
				if(System.IO.File.Exists(unityOutputManifestFilePath) == false)
					throw new Exception("构建过程中发生严重错误！请查阅上下文日志！");
			}

			BuildRunner.Log("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new BuildResultContext();
			buildResultContext.UnityManifest = buildResults;
			context.SetContextObject(buildResultContext);

			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyRawBundle(buildMapContext, buildParametersContext);
				UpdateBuildBundleInfo(buildMapContext, buildParametersContext, buildResultContext);
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

		/// <summary>
		/// 更新构建结果
		/// </summary>
		private void UpdateBuildBundleInfo(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext, BuildResultContext buildResult)
		{
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string filePath = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
					bundleInfo.ContentHash = HashUtility.FileMD5(filePath);
				}
				else
				{
					var hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
					if (hash.isValid)
						bundleInfo.ContentHash = hash.ToString();
					else
						throw new Exception($"Not found bundle in build result : {bundleInfo.BundleName}");
				}
			}
		}
	}
}