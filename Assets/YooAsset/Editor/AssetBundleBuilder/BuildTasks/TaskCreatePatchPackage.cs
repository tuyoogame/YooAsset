using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("制作补丁包")]
	public class TaskCreatePatchPackage : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyPatchFiles(buildParameters);
			}
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPatchFiles(BuildParametersContext buildParametersContext)
		{
			var buildParameters = buildParametersContext.Parameters;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			BuildRunner.Log($"开始拷贝补丁文件到补丁包目录：{packageOutputDirectory}");

			// 拷贝Report文件
			{
				string fileName = YooAssetSettingsData.GetReportFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string sourcePath = $"{pipelineOutputDirectory}/{fileName}";
				string destPath = $"{packageOutputDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string sourcePath = $"{pipelineOutputDirectory}/{fileName}";
				string destPath = $"{packageOutputDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单哈希文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string sourcePath = $"{pipelineOutputDirectory}/{fileName}";
				string destPath = $"{packageOutputDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单版本文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestVersionFileName(buildParameters.PackageName);
				string sourcePath = $"{pipelineOutputDirectory}/{fileName}";
				string destPath = $"{packageOutputDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				// 拷贝构建日志
				{
					string sourcePath = $"{pipelineOutputDirectory}/buildlogtep.json";
					string destPath = $"{packageOutputDirectory}/buildlogtep.json";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝代码防裁剪配置
				if (buildParameters.SBPParameters.WriteLinkXML)
				{
					string sourcePath = $"{pipelineOutputDirectory}/link.xml";
					string destPath = $"{packageOutputDirectory}/link.xml";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}
			else
			{
				// 拷贝UnityManifest序列化文件
				{
					string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
					string destPath = $"{packageOutputDirectory}/{YooAssetSettings.OutputFolderName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝UnityManifest文本文件
				{
					string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
					string destPath = $"{packageOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}

			// 拷贝所有补丁文件
			int progressValue = 0;
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(pipelineOutputDirectory, buildParameters.PackageName, buildParameters.PackageVersion);
			int patchFileTotalCount = patchManifest.BundleList.Count;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				string sourcePath = $"{pipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{packageOutputDirectory}/{patchBundle.FileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
			}
			EditorTools.ClearProgressBar();
		}
	}
}