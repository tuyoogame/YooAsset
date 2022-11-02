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
				bundleInfo.LoadMethod = EBundleLoadMethod.Normal;

				EncryptFileInfo fileInfo = new EncryptFileInfo();
				fileInfo.BundleName = bundleInfo.BundleName;
				fileInfo.FilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";

				var encryptResult = encryptionServices.Encrypt(fileInfo);		
				if (encryptResult.LoadMethod != EBundleLoadMethod.Normal)
				{
					// 注意：原生文件不支持加密
					if (bundleInfo.IsRawFile)
					{
						UnityEngine.Debug.LogWarning($"Encryption not support raw file : {bundleInfo.BundleName}");
						continue;
					}

					string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
					FileUtility.CreateFile(filePath, encryptResult.EncryptedData);
					bundleInfo.EncryptedFilePath = filePath;
					bundleInfo.LoadMethod = encryptResult.LoadMethod;
					BuildRunner.Log($"Bundle文件加密完成：{filePath}");
				}

				// 进度条
				EditorTools.DisplayProgressBar("加密资源包", ++progressValue, buildMapContext.BundleInfos.Count);
			}
			EditorTools.ClearProgressBar();
		}
	}
}