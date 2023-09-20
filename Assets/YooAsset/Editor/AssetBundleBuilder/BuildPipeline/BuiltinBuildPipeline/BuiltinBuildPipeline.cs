using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuiltinBuildPipeline : IBuildPipeline
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
					new TaskPrepare_BBP(), //前期准备工作
					new TaskGetBuildMap_BBP(), //获取构建列表
					new TaskBuilding_BBP(), //开始执行构建
					new TaskVerifyBuildResult_BBP(), //验证构建结果
					new TaskUpdateBundleInfo_BBP(), //更新资源包信息
					new TaskCreateManifest_BBP(), //创建清单文件
					new TaskCreateReport_BBP(), //创建报告文件
					new TaskCreatePackage_BBP(), //制作包裹
					new TaskCopyBuildinFiles_BBP(), //拷贝内置文件
				};
			return pipeline;
		}
	}
}