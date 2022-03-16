using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	/// <summary>
	/// 创建说明文件
	/// </summary>
	public class TaskCreateReadme : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();
			CreateReadmeFile(buildParameters, buildMapContext);
		}

		/// <summary>
		/// 创建Readme文件到输出目录
		/// </summary>
		private void CreateReadmeFile(AssetBundleBuilder.BuildParametersContext buildParameters, TaskGetBuildMap.BuildMapContext buildMapContext)
		{
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory);

			// 删除旧文件
			string filePath = $"{buildParameters.PipelineOutputDirectory}/{ResourceSettingData.Setting.ReadmeFileName}";
			if (File.Exists(filePath))
				File.Delete(filePath);

			UnityEngine.Debug.Log($"创建说明文件：{filePath}");

			StringBuilder content = new StringBuilder();
			AppendData(content, $"构建时间：{DateTime.Now}");
			AppendData(content, $"构建平台：{buildParameters.Parameters.BuildTarget}");
			AppendData(content, $"构建版本：{buildParameters.Parameters.BuildVersion}");
			AppendData(content, $"冗余机制：{buildParameters.Parameters.ApplyRedundancy}");

			AppendData(content, "");
			AppendData(content, $"--着色器--");
			AppendData(content, $"IsCollectAllShaders：{AssetBundleCollectorSettingData.Setting.IsCollectAllShaders}");
			AppendData(content, $"ShadersBundleName：{AssetBundleCollectorSettingData.Setting.ShadersBundleName}");

			AppendData(content, "");
			AppendData(content, $"--配置信息--");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = AssetBundleCollectorSettingData.Setting.Collectors[i];
				AppendData(content, wrapper.ToString());
			}

			AppendData(content, "");
			AppendData(content, $"--构建参数--");
			AppendData(content, $"CompressOption：{buildParameters.Parameters.CompressOption}");
			AppendData(content, $"IsForceRebuild：{buildParameters.Parameters.IsForceRebuild}");
			AppendData(content, $"BuildinTags：{buildParameters.Parameters.BuildinTags}");
			AppendData(content, $"IsAppendHash：{buildParameters.Parameters.IsAppendHash}");
			AppendData(content, $"IsDisableWriteTypeTree：{buildParameters.Parameters.IsDisableWriteTypeTree}");
			AppendData(content, $"IsIgnoreTypeTreeChanges：{buildParameters.Parameters.IsIgnoreTypeTreeChanges}");
			AppendData(content, $"IsDisableLoadAssetByFileName : {buildParameters.Parameters.IsDisableLoadAssetByFileName}");

			AppendData(content, "");
			AppendData(content, $"--构建信息--");
			AppendData(content, $"参与构建的资源总数：{buildMapContext.GetAllAssets().Count}");
			GetBundleFileCountAndTotalSize(patchManifest, out int fileCount1, out long fileTotalSize1);
			AppendData(content, $"构建的资源包总数：{fileCount1} 文件总大小：{fileTotalSize1 / (1024 * 1024)}MB");
			GetBuildinFileCountAndTotalSize(patchManifest, out int fileCount2, out long fileTotalSize2);
			AppendData(content, $"内置的资源包总数：{fileCount2} 文件总大小：{fileTotalSize2 / (1024 * 1024)}MB");
			GetNotBuildinFileCountAndTotalSize(patchManifest, out int fileCount3, out long fileTotalSize3);
			AppendData(content, $"非内置的资源包总数：{fileCount3} 文件总大小：{fileTotalSize3 / (1024 * 1024)}MB");
			GetEncryptedFileCountAndTotalSize(patchManifest, out int fileCount4, out long fileTotalSize4);
			AppendData(content, $"加密的资源包总数：{fileCount4} 文件总大小：{fileTotalSize4 / (1024 * 1024)}MB");
			GetRawFileCountAndTotalSize(patchManifest, out int fileCount5, out long fileTotalSize5);
			AppendData(content, $"原生的资源包总数：{fileCount5} 文件总大小：{fileTotalSize5 / (1024 * 1024)}MB");

			AppendData(content, "");
			AppendData(content, $"--冗余列表--");
			for (int i = 0; i < buildMapContext.RedundancyList.Count; i++)
			{
				string redundancyAssetPath = buildMapContext.RedundancyList[i];
				AppendData(content, redundancyAssetPath);
			}

			AppendData(content, "");
			AppendData(content, $"--构建列表--");
			for (int i = 0; i < buildMapContext.BundleInfos.Count; i++)
			{
				string bundleName = buildMapContext.BundleInfos[i].BundleName;
				AppendData(content, bundleName);
			}

			AppendData(content, "");
			AppendData(content, $"--内置文件列表--");
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin)
				{
					AppendData(content, patchBundle.BundleName);
				}
			}

			AppendData(content, "");
			AppendData(content, $"--非内置文件列表--");
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin == false)
				{
					AppendData(content, patchBundle.BundleName);
				}
			}

			AppendData(content, "");
			AppendData(content, $"--加密文件列表--");
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsEncrypted)
				{
					AppendData(content, patchBundle.BundleName);
				}
			}

			AppendData(content, "");
			AppendData(content, $"--原生文件列表--");
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsRawFile)
				{
					AppendData(content, patchBundle.BundleName);
				}
			}

			// 创建新文件
			File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
		}
		private void AppendData(StringBuilder sb, string data)
		{
			sb.Append(data);
			sb.Append("\r\n");
		}

		private void GetBundleFileCountAndTotalSize(PatchManifest patchManifest, out int fileCount, out long fileBytes)
		{
			fileCount = patchManifest.BundleList.Count;
			fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				fileBytes += patchBundle.SizeBytes;
			}
		}
		private void GetBuildinFileCountAndTotalSize(PatchManifest patchManifest, out int fileCount, out long fileBytes)
		{
			fileCount = 0;
			fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin)
				{
					fileCount++;
					fileBytes += patchBundle.SizeBytes;
				}
			}
		}
		private void GetNotBuildinFileCountAndTotalSize(PatchManifest patchManifest, out int fileCount, out long fileBytes)
		{
			fileCount = 0;
			fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin == false)
				{
					fileCount++;
					fileBytes += patchBundle.SizeBytes;
				}
			}
		}
		private void GetEncryptedFileCountAndTotalSize(PatchManifest patchManifest, out int fileCount, out long fileBytes)
		{
			fileCount = 0;
			fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsEncrypted)
				{
					fileCount++;
					fileBytes += patchBundle.SizeBytes;
				}
			}
		}
		private void GetRawFileCountAndTotalSize(PatchManifest patchManifest, out int fileCount, out long fileBytes)
		{
			fileCount = 0;
			fileBytes = 0;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsRawFile)
				{
					fileCount++;
					fileBytes += patchBundle.SizeBytes;
				}
			}
		}
	}
}