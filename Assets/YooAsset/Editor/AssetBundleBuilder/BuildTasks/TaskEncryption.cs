using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("资源包加密")]
	public class TaskEncryption : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				EncryptingBundleFiles(buildParameters, buildMapContext);
			}
		}

		/// <summary>
		/// 加密文件
		/// </summary>
		private void EncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			var encryptionServices = buildParametersContext.Parameters.EncryptionServices;

			// 如果没有设置加密类
			if (encryptionServices == null)
				return;

			int progressValue = 0;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (encryptionServices.Check(bundleInfo.BundleName))
				{
					if (bundleInfo.IsRawFile)
					{
						UnityEngine.Debug.LogWarning($"Encryption not support raw file : {bundleInfo.BundleName}");
						continue;
					}

					string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
					string savePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
					byte[] fileData = File.ReadAllBytes(filePath);
					byte[] encryptData = encryptionServices.Encrypt(fileData);
					FileUtility.CreateFile(savePath, encryptData);
					bundleInfo.EncryptedFilePath = savePath;
					BuildRunner.Log($"Bundle文件加密完成：{savePath}");
				}

				// 进度条
				EditorTools.DisplayProgressBar("加密资源包", ++progressValue, buildMapContext.BundleInfos.Count);
			}
			EditorTools.ClearProgressBar();
		}
	}
}