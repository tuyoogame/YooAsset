using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace YooAsset.Editor
{
	public class BuildRunner
	{
		public static bool EnableLog = true;

		/// <summary>
		/// 执行构建流程
		/// </summary>
		/// <returns>如果成功返回TRUE，否则返回FALSE</returns>
		public static BuildResult Run(List<IBuildTask> pipeline, BuildContext context)
		{
			if (pipeline == null)
				throw new ArgumentNullException("pipeline");
			if (context == null)
				throw new ArgumentNullException("context");

			BuildResult buildResult = new BuildResult();
			buildResult.Success = true;
			for (int i = 0; i < pipeline.Count; i++)
			{
				IBuildTask task = pipeline[i];
				try
				{
					var taskAttribute = task.GetType().GetCustomAttribute<TaskAttribute>();
					Log($"---------------------------------------->{taskAttribute.Desc}<---------------------------------------");
					task.Run(context);
				}
				catch (Exception e)
				{
					buildResult.FailedTask = task.GetType().Name;
					buildResult.FailedInfo = e.ToString();
					buildResult.Success = false;
					break;
				}
			}

			// 返回运行结果
			return buildResult;
		}

		/// <summary>
		/// 日志输出
		/// </summary>
		public static void Log(string info)
		{
			if (EnableLog)
			{
				UnityEngine.Debug.Log(info);
			}
		}

		/// <summary>
		/// 日志输出
		/// </summary>
		public static void Info(string info)
		{
			UnityEngine.Debug.Log(info);
		}
	}
}