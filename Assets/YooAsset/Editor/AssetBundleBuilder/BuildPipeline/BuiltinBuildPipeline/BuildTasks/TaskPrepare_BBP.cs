using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建准备工作")]
	public class TaskPrepare_BBP : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = buildParametersContext.Parameters;
			var builtinBuildParameters = buildParameters as BuiltinBuildParameters;

			// 检测基础构建参数
			buildParametersContext.CheckBuildParameters();

			// 检测Unity版本
#if UNITY_2021_3_OR_NEWER
			if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
			{
				BuildLogger.Warning("Unity2021版本开始内置构建管线不再维护，推荐使用可编程构建管线（SBP）！");
			}
#endif
		}
	}
}