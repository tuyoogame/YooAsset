using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	/// <summary>
	/// 拷贝内置文件到StreamingAssets
	/// </summary>
	public class TaskCopyBuildinFiles : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			// 注意：我们只有在强制重建的时候才会拷贝
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			if(buildParameters.Parameters.IsForceRebuild)
			{
				// 清空流目录
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();

				// 拷贝内置文件
				var pipelineOutputDirectory = buildParameters.PipelineOutputDirectory;
				CopyBuildinFilesToStreaming(pipelineOutputDirectory);
			}
		}

		private void CopyBuildinFilesToStreaming(string pipelineOutputDirectory)
		{
			// 加载补丁清单
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(pipelineOutputDirectory);

			// 拷贝文件列表
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin == false)
					continue;

				string sourcePath = $"{pipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{Application.dataPath}/StreamingAssets/{patchBundle.Hash}";
				Debug.Log($"拷贝内置文件到流目录：{patchBundle.BundleName}");
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝清单文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{ResourceSettingData.Setting.PatchManifestFileName}";
				string destPath = $"{Application.dataPath}/StreamingAssets/{ResourceSettingData.Setting.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝清单哈希文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{ResourceSettingData.Setting.PatchManifestHashFileName}";
				string destPath = $"{Application.dataPath}/StreamingAssets/{ResourceSettingData.Setting.PatchManifestHashFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 刷新目录
			AssetDatabase.Refresh();
		}
	}
}