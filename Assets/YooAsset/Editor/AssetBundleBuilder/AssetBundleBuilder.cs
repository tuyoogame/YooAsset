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
		/// 开始构建
		/// </summary>
		public BuildResult Run(BuildParameters buildParameters)
		{
			// 清空旧数据
			_buildContext.ClearAllContext();

			// 检测构建参数是否为空
			if (buildParameters == null)
				throw new Exception($"{nameof(buildParameters)} is null !");

			// 检测可编程构建管线参数
			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if (buildParameters.SBPParameters == null)
					throw new Exception($"{nameof(BuildParameters.SBPParameters)} is null !");

				if (buildParameters.BuildMode == EBuildMode.DryRunBuild)
					throw new Exception($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !");

				if (buildParameters.BuildMode == EBuildMode.ForceRebuild)
					throw new Exception($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.ForceRebuild)} build mode !");
			}

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 初始化日志
			BuildLogger.InitLogger(buildParameters.EnableLog);

			// 执行构建流程
			var pipeline = GetBuildTasks(buildParameters.BuildPipeline);
			var buildResult = BuildRunner.Run(pipeline, _buildContext);
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

		private List<IBuildTask> GetBuildTasks(EBuildPipeline buildPipeline)
		{
			// 获取任务节点的属性集合
			List<TaskAttribute> attrList = new List<TaskAttribute>();
			if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				/*
				List<IBuildTask> pipeline = new List<IBuildTask>
				{
					new TaskPrepare(), //前期准备工作
					new TaskGetBuildMap(), //获取构建列表
					new TaskBuilding(), //开始执行构建
					new TaskCopyRawFile(), //拷贝原生文件
					new TaskVerifyBuildResult(), //验证构建结果
					new TaskEncryption(), //加密资源文件
					new TaskUpdateBundleInfo(), //更新资源包信息
					new TaskCreateManifest(), //创建清单文件
					new TaskCreateReport(), //创建报告文件
					new TaskCreatePackage(), //制作包裹
					new TaskCopyBuildinFiles(), //拷贝内置文件
				};
				*/

				var classTypes = EditorTools.GetAssignableTypes(typeof(IBuildTask));
				foreach (var classType in classTypes)
				{
					var attribute = classType.GetCustomAttribute<TaskAttribute>();
					if (attribute == null)
						throw new Exception($"Not found {nameof(TaskAttribute)} int type : {classType.FullName}");

					attribute.ClassType = classType;
					if (attribute.Pipeline == ETaskPipeline.AllPipeline || attribute.Pipeline == ETaskPipeline.BuiltinBuildPipeline)
						attrList.Add(attribute);
				}
			}
			else if (buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				/*
				List<IBuildTask> pipeline = new List<IBuildTask>
				{
					new TaskPrepare(), //前期准备工作
					new TaskGetBuildMap(), //获取构建列表
					new TaskBuilding_SBP(), //开始执行构建
					new TaskCopyRawFile(), //拷贝原生文件
					new TaskVerifyBuildResult_SBP(), //验证构建结果
					new TaskEncryption(), //加密资源文件
					new TaskUpdateBundleInfo(), //更新补丁信息
					new TaskCreateManifest(), //创建清单文件
					new TaskCreateReport(), //创建报告文件
					new TaskCreatePackage(), //制作补丁包
					new TaskCopyBuildinFiles(), //拷贝内置文件
				};
				*/

				var classTypes = EditorTools.GetAssignableTypes(typeof(IBuildTask));
				foreach (var classType in classTypes)
				{
					var attribute = classType.GetCustomAttribute<TaskAttribute>();
					if (attribute == null)
						throw new Exception($"Not found {nameof(TaskAttribute)} int type : {classType.FullName}");

					attribute.ClassType = classType;
					if (attribute.Pipeline == ETaskPipeline.AllPipeline || attribute.Pipeline == ETaskPipeline.ScriptableBuildPipeline)
						attrList.Add(attribute);
				}
			}
			else
			{
				throw new NotImplementedException();
			}

			// 对任务节点进行排序
			attrList.Sort((a, b) =>
			{
				if (a.TaskOrder > b.TaskOrder) { return 1; }
				else if (a.TaskOrder < b.TaskOrder) { return -1; }
				else { return 0; }
			});

			// 创建任务节点实例
			List<IBuildTask> pipeline = new List<IBuildTask>(attrList.Count);
			foreach (var taskAttr in attrList)
			{
				var task = Activator.CreateInstance(taskAttr.ClassType) as IBuildTask;
				pipeline.Add(task);
			}

			return pipeline;
		}
	}
}