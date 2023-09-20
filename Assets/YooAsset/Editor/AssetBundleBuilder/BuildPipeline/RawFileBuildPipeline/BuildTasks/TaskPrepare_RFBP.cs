using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建准备工作")]
	public class TaskPrepare_RFBP : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = buildParametersContext.Parameters;

			// 检测基础构建参数
			buildParametersContext.CheckBuildParameters();

			// 检测不被支持的构建模式
			if (buildParameters.BuildMode == EBuildMode.DryRunBuild)
				throw new Exception($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !");
			if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
				throw new Exception($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.IncrementalBuild)} build mode !");
		}
	}
}