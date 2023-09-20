using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
	public class ScriptableBuildPipeline : IBuildPipeline
	{
		public BuildResult Run(BuildParameters buildParameters, bool enableLog)
		{
			AssetBundleBuilder builder = new AssetBundleBuilder();
			return builder.Run(buildParameters, GetDefaultBuildPipeline(), enableLog);
		}

		/// <summary>
		/// 获取默认的构建流程
		/// </summary>
		private List<IBuildTask> GetDefaultBuildPipeline()
		{
			List<IBuildTask> pipeline = new List<IBuildTask>
				{
					new TaskPrepare_SBP(), //前期准备工作
					new TaskGetBuildMap_SBP(), //获取构建列表
					new TaskBuilding_SBP(), //开始执行构建
					new TaskVerifyBuildResult_SBP(), //验证构建结果
					new TaskUpdateBundleInfo_SBP(), //更新补丁信息
					new TaskCreateManifest_SBP(), //创建清单文件
					new TaskCreateReport_SBP(), //创建报告文件
					new TaskCreatePackage_SBP(), //制作补丁包
					new TaskCopyBuildinFiles_SBP(), //拷贝内置文件
				};
			return pipeline;
		}
	}
}