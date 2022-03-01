using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	/// <summary>
	/// 制作补丁包
	/// </summary>
	public class TaskCreatePatchPackage : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			CopyPatchFiles(buildParameters);
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPatchFiles(AssetBundleBuilder.BuildParametersContext buildParameters)
		{
			string packageDirectory = buildParameters.GetPackageDirectory();
			UnityEngine.Debug.Log($"开始拷贝补丁文件到补丁包目录：{packageDirectory}");

			// 拷贝Readme文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.ReadmeFileName}";
				string destPath = $"{packageDirectory}/{ResourceSettingData.Setting.ReadmeFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝Readme文件到：{destPath}");
			}

			// 拷贝PatchManifest文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.PatchManifestFileName}";
				string destPath = $"{packageDirectory}/{ResourceSettingData.Setting.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝PatchManifest文件到：{destPath}");
			}

			// 拷贝PatchManifest哈希文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.PatchManifestHashFileName}";
				string destPath = $"{packageDirectory}/{ResourceSettingData.Setting.PatchManifestHashFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝PatchManifest哈希文件到：{destPath}");
			}

			// 拷贝UnityManifest序列化文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.UnityManifestFileName}";
				string destPath = $"{packageDirectory}/{ResourceSettingData.Setting.UnityManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝UnityManifest文件到：{destPath}");
			}

			// 拷贝UnityManifest文本文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.UnityManifestFileName}.manifest";
				string destPath = $"{packageDirectory}/{ResourceSettingData.Setting.UnityManifestFileName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝所有补丁文件
			// 注意：拷贝的补丁文件都是需要玩家热更新的文件
			int progressValue = 0;
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory);
			int patchFileTotalCount = patchManifest.BundleList.Count;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.Version == buildParameters.Parameters.BuildVersion)
				{
					string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{patchBundle.BundleName}";
					string destPath = $"{packageDirectory}/{patchBundle.Hash}";
					EditorTools.CopyFile(sourcePath, destPath, true);
					UnityEngine.Debug.Log($"拷贝补丁文件到补丁包：{patchBundle.BundleName}");
					EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
				}
			}
			EditorTools.ClearProgressBar();
		}
	}
}