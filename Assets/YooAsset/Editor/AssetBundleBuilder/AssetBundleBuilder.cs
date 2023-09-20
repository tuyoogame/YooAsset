using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilder
	{
		private readonly BuildContext _buildContext = new BuildContext();

		/// <summary>
		/// 构建资源包
		/// </summary>
		public BuildResult Run(BuildParameters buildParameters, List<IBuildTask> buildPipeline, bool enableLog)
		{
			// 检测构建参数是否为空
			if (buildParameters == null)
				throw new Exception($"{nameof(buildParameters)} is null !");

			// 检测构建参数是否为空
			if (buildPipeline.Count == 0)
				throw new Exception($"Build pipeline is empty !");	

			// 清空旧数据
			_buildContext.ClearAllContext();

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 初始化日志
			BuildLogger.InitLogger(enableLog);

			// 执行构建流程
			Debug.Log($"Begin to build package : {buildParameters.PackageName} by {buildParameters.BuildPipeline}");
			var buildResult = BuildRunner.Run(buildPipeline, _buildContext);
			if (buildResult.Success)
			{
				buildResult.OutputPackageDirectory = buildParametersContext.GetPackageOutputDirectory();
				BuildLogger.Log($"{buildParameters.BuildMode} pipeline build succeed !");
			}
			else
			{
				BuildLogger.Warning($"{buildParameters.BuildMode} pipeline build failed !");
				BuildLogger.Error($"Build task failed : {buildResult.FailedTask}");
				BuildLogger.Error(buildResult.ErrorInfo);
			}

			return buildResult;
		}
	}
}