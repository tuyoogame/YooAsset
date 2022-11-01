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
			var patchManifestContext = context.GetContextObject<PatchManifestContext>();
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				if (buildParametersContext.Parameters.CopyBuildinFileOption != ECopyBuildinFileOption.None)
				{
					CopyBuildinFilesToStreaming(buildParametersContext, patchManifestContext);
				}
			}
		}

		/// <summary>
		/// 拷贝首包资源文件
		/// </summary>
		private void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext, PatchManifestContext patchManifestContext)
		{
			ECopyBuildinFileOption option = buildParametersContext.Parameters.CopyBuildinFileOption;
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			string streamingAssetsDirectory = AssetBundleBuilderHelper.GetStreamingAssetsFolderPath();
			string buildPackageName = buildParametersContext.Parameters.PackageName;
			string buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

			// 加载补丁清单
			PatchManifest patchManifest = patchManifestContext.Manifest;

			// 清空流目录
			if (option == ECopyBuildinFileOption.ClearAndCopyAll || option == ECopyBuildinFileOption.ClearAndCopyByTags)
			{
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();
			}

			// 拷贝补丁清单文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(buildPackageName, buildPackageVersion);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单哈希文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestHashFileName(buildPackageName, buildPackageVersion);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单版本文件
			{
				string fileName = YooAssetSettingsData.GetPatchManifestVersionFileName(buildPackageName);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝文件列表（所有文件）
			if (option == ECopyBuildinFileOption.ClearAndCopyAll || option == ECopyBuildinFileOption.OnlyCopyAll)
			{		
				foreach (var patchBundle in patchManifest.BundleList)
				{
					string sourcePath = $"{packageOutputDirectory}/{patchBundle.FileName}";
					string destPath = $"{streamingAssetsDirectory}/{patchBundle.FileName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}

			// 拷贝文件列表（带标签的文件）
			if (option == ECopyBuildinFileOption.ClearAndCopyByTags || option == ECopyBuildinFileOption.OnlyCopyByTags)
			{
				string[] tags = buildParametersContext.Parameters.CopyBuildinFileTags.Split(';');
				foreach (var patchBundle in patchManifest.BundleList)
				{
					if (patchBundle.HasTag(tags) == false)
						continue;
					string sourcePath = $"{packageOutputDirectory}/{patchBundle.FileName}";
					string destPath = $"{streamingAssetsDirectory}/{patchBundle.FileName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}

			// 刷新目录
			AssetDatabase.Refresh();
			BuildRunner.Log($"内置文件拷贝完成：{streamingAssetsDirectory}");
		}
	}
}