using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("制作包裹")]
	public class TaskCreatePackage_SBP : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode != EBuildMode.SimulateBuild)
			{
				CopyPackageFiles(buildParameters, buildMapContext);
			}
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPackageFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			BuildLogger.Log($"开始拷贝补丁文件到补丁包目录：{packageOutputDirectory}");

			// 拷贝构建日志
			{
				string sourcePath = $"{pipelineOutputDirectory}/buildlogtep.json";
				string destPath = $"{packageOutputDirectory}/buildlogtep.json";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝代码防裁剪配置
			if (scriptableBuildParameters.WriteLinkXML)
			{
				string sourcePath = $"{pipelineOutputDirectory}/link.xml";
				string destPath = $"{packageOutputDirectory}/link.xml";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝所有补丁文件
			int progressValue = 0;
			int fileTotalCount = buildMapContext.Collection.Count;
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				EditorTools.CopyFile(bundleInfo.PackageSourceFilePath, bundleInfo.PackageDestFilePath, true);
				EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, fileTotalCount);
			}
			EditorTools.ClearProgressBar();
		}
	}
}