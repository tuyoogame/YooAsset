using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilder
	{
		private readonly BuildContext _buildContext = new BuildContext();

		/// <summary>
		/// 开始构建
		/// </summary>
		public bool Run(BuildParameters buildParameters)
		{
			// 清空旧数据
			_buildContext.ClearAllContext();

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 执行构建流程
			List<IBuildTask> pipeline = new List<IBuildTask>
			{
				new TaskPrepare(), //前期准备工作
				new TaskGetBuildMap(), //获取构建列表
				new TaskBuilding(), //开始执行构建
				new TaskVerifyBuildResult(), //验证构建结果
				new TaskEncryption(), //加密资源文件
				new TaskCreatePatchManifest(), //创建清单文件
				new TaskCreateReport(), //创建报告文件
				new TaskCreatePatchPackage(), //制作补丁包
				new TaskCopyBuildinFiles(), //拷贝内置文件
			};

			if (buildParameters.BuildMode == EBuildMode.SimulateBuild)
				BuildRunner.EnableLog = false;
			else
				BuildRunner.EnableLog = true;

			bool succeed = BuildRunner.Run(pipeline, _buildContext);
			if (succeed)
				Debug.Log($"{buildParameters.BuildMode} pipeline build succeed !");
			else
				Debug.LogWarning($"{buildParameters.BuildMode} pipeline build failed !");
			return succeed;
		}
	}
}