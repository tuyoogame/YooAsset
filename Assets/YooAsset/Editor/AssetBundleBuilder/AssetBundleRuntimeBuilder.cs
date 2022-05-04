using System;
using UnityEditor;

namespace YooAsset.Editor
{
	public static class AssetBundleRuntimeBuilder
	{
		private static string _manifestFilePath = string.Empty;

		/// <summary>
		/// 快速模式构建
		/// </summary>
		public static void FastBuild()
		{
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			BuildParameters buildParameters = new BuildParameters();
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
			buildParameters.BuildMode = EBuildMode.FastRunBuild;
			buildParameters.BuildVersion = AssetBundleBuilderSettingData.Setting.BuildVersion;
			buildParameters.BuildinTags = AssetBundleBuilderSettingData.Setting.BuildTags;
			buildParameters.EnableAddressable = AssetBundleCollectorSettingData.Setting.EnableAddressable;

			AssetBundleBuilder builder = new AssetBundleBuilder();
			bool buildResult = builder.Run(buildParameters);
			if (buildResult)
			{
				string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(buildParameters.OutputRoot, buildParameters.BuildTarget);
				_manifestFilePath = $"{pipelineOutputDirectory}_{EBuildMode.FastRunBuild}/{YooAssetSettingsData.GetPatchManifestFileName(buildParameters.BuildVersion)}";
			}
			else
			{
				_manifestFilePath = null;
			}
		}
		
		/// <summary>
		/// 获取构建的补丁清单文件路径
		/// </summary>
		public static string GetPatchManifestFilePath()
		{
			return _manifestFilePath;
		}
	}
}