using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[TaskAttribute("拷贝内置文件到流目录")]
	public class TaskCopyBuildinFiles : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			if (buildParametersContext.Parameters.CopyBuildinTagFiles)
			{
				// 清空流目录
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();

				// 拷贝内置文件
				CopyBuildinFilesToStreaming(buildParametersContext);
			}
		}

		private void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext)
		{
			string streamingAssetsOutputDirectory = AssetBundleBuilderHelper.GetStreamingAssetsFolderPath();
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageName = buildParametersContext.Parameters.BuildPackage;
			string packageCRC = buildParametersContext.OutputPackageCRC;

			// 加载补丁清单
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(pipelineOutputDirectory, packageName, packageCRC);

			// 拷贝文件列表
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin == false)
					continue;

				string sourcePath = $"{pipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{streamingAssetsOutputDirectory}/{patchBundle.FileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝清单文件
			{
				string manifestFileName = YooAssetSettingsData.GetPatchManifestFileName(packageName, packageCRC);
				string sourcePath = $"{pipelineOutputDirectory}/{manifestFileName}";
				string destPath = $"{streamingAssetsOutputDirectory}/{manifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝静态版本文件
			{
				string versionFileName = YooAssetSettingsData.GetStaticVersionFileName(packageName);
				string sourcePath = $"{pipelineOutputDirectory}/{versionFileName}";
				string destPath = $"{streamingAssetsOutputDirectory}/{versionFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 刷新目录
			AssetDatabase.Refresh();
			BuildRunner.Log($"内置文件拷贝完成：{streamingAssetsOutputDirectory}");
		}
	}
}