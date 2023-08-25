using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("更新资源包信息")]
	public class TaskUpdateBundleInfo : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			int outputNameStyle = (int)buildParametersContext.Parameters.OutputNameStyle;

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
				if (bundleInfo.IsEncryptedFile)
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

		private string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var parameters = buildParametersContext.Parameters;
			var buildMode = parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000000000000000000000000000"; //32位

			if (bundleInfo.IsRawFile)
			{
				string filePath = bundleInfo.PackageSourceFilePath;
				return HashUtility.FileMD5(filePath);
			}

			if (parameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				var buildResult = context.GetContextObject<TaskBuilding.BuildResultContext>();
				var hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
				if (hash.isValid)
					return hash.ToString();
				else
					throw new Exception($"Not found bundle hash in build result : {bundleInfo.BundleName}");
			}
			else if (parameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				// 注意：当资源包的依赖列表发生变化的时候，ContentHash也会发生变化！
				var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
				if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
					return value.Hash.ToString();
				else
					throw new Exception($"Not found bundle hash in build result : {bundleInfo.BundleName}");
			}
			else
			{
				throw new System.NotImplementedException();
			}
		}
		private uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var parameters = buildParametersContext.Parameters;
			var buildMode = parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return 0;

			if (bundleInfo.IsRawFile)
				return 0;

			if (parameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				string filePath = bundleInfo.BuildOutputFilePath;
				if (BuildPipeline.GetCRCForAssetBundle(filePath, out uint crc))
					return crc;
				else
					throw new Exception($"Not found bundle crc in build result : {bundleInfo.BundleName}");
			}
			else if (parameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
				if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
					return value.Crc;
				else
					throw new Exception($"Not found bundle crc in build result : {bundleInfo.BundleName}");
			}
			else
			{
				throw new System.NotImplementedException();
			}
		}
		private string GetBundleFileHash(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000000000000000000000000000"; //32位
			else
				return HashUtility.FileMD5(filePath);
		}
		private string GetBundleFileCRC(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000"; //8位
			else
				return HashUtility.FileCRC32(filePath);
		}
		private long GetBundleFileSize(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return 0;
			else
				return FileUtility.GetFileSize(filePath);
		}
	}
}