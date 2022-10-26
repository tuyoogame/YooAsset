using UnityEditor;

namespace YooAsset.Editor
{
	public static class AssetBundleSimulateBuilder
	{
		/// <summary>
		/// 模拟构建
		/// </summary>
		public static string SimulateBuild(string packageName)
		{
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			BuildParameters buildParameters = new BuildParameters();
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
			buildParameters.BuildMode = EBuildMode.SimulateBuild;
			buildParameters.PackageName = packageName;
			buildParameters.PackageVersion = "Simulate";

			AssetBundleBuilder builder = new AssetBundleBuilder();
			var buildResult = builder.Run(buildParameters);
			if (buildResult.Success)
			{
				string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(buildParameters.OutputRoot, buildParameters.PackageName, buildParameters.BuildTarget, buildParameters.BuildMode);
				string manifestFileName = YooAssetSettingsData.GetPatchManifestFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string manifestFilePath = $"{pipelineOutputDirectory}/{manifestFileName}";
				return manifestFilePath;
			}
			else
			{
				return null;
			}
		}
	}
}