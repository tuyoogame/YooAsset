using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
	public class RawFileBuildPipeline : IBuildPipeline
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
					new TaskPrepare_RFBP(), //前期准备工作
					new TaskGetBuildMap_RFBP(), //获取构建列表
					new TaskBuilding_RFBP(), //开始执行构建
					new TaskUpdateBundleInfo_RFBP(), //更新资源包信息
					new TaskCreateManifest_RFBP(), //创建清单文件
					new TaskCreateReport_RFBP(), //创建报告文件
					new TaskCreatePackage_RFBP(), //制作包裹
					new TaskCopyBuildinFiles_RFBP(), //拷贝内置文件
				};
			return pipeline;
		}
	}
}