using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public abstract class TaskUpdateBundleInfo
	{
		public void UpdateBundleInfo(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			int outputNameStyle = (int)buildParametersContext.Parameters.FileNameStyle;

			// 1.检测文件名长度
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				// NOTE：检测文件名长度不要超过260字符。
				string fileName = bundleInfo.BundleName;
				if (fileName.Length >= 260)
					throw new Exception($"The output bundle name is too long {fileName.Length} chars : {fileName}");
			}

			// 2.更新构建输出的文件路径
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				bundleInfo.BuildOutputFilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
				if (bundleInfo.Encrypted)
					bundleInfo.PackageSourceFilePath = bundleInfo.EncryptedFilePath;
				else
					bundleInfo.PackageSourceFilePath = bundleInfo.BuildOutputFilePath;
			}

			// 3.更新文件其它信息
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				bundleInfo.PackageUnityHash = GetUnityHash(bundleInfo, context);
				bundleInfo.PackageUnityCRC = GetUnityCRC(bundleInfo, context);
				bundleInfo.PackageFileHash = GetBundleFileHash(bundleInfo.PackageSourceFilePath, buildParametersContext);
				bundleInfo.PackageFileCRC = GetBundleFileCRC(bundleInfo.PackageSourceFilePath, buildParametersContext);
				bundleInfo.PackageFileSize = GetBundleFileSize(bundleInfo.PackageSourceFilePath, buildParametersContext);
			}

			// 4.更新补丁包输出的文件路径
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				string bundleName = bundleInfo.BundleName;
				string fileHash = bundleInfo.PackageFileHash;
				string fileExtension = ManifestTools.GetRemoteBundleFileExtension(bundleName);
				string fileName = ManifestTools.GetRemoteBundleFileName(outputNameStyle, bundleName, fileExtension, fileHash);
				bundleInfo.PackageDestFilePath = $"{packageOutputDirectory}/{fileName}";
			}
		}

		protected abstract string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context);
		protected abstract uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context);
		protected abstract string GetBundleFileHash(string filePath, BuildParametersContext buildParametersContext);
		protected abstract string GetBundleFileCRC(string filePath, BuildParametersContext buildParametersContext);
		protected abstract long GetBundleFileSize(string filePath, BuildParametersContext buildParametersContext);
	}
}