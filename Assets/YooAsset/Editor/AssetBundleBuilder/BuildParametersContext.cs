using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildParametersContext : IContextObject
	{
		private readonly System.Diagnostics.Stopwatch _buildWatch = new System.Diagnostics.Stopwatch();

		/// <summary>
		/// 构建参数
		/// </summary>
		public BuildParameters Parameters { private set; get; }

		/// <summary>
		/// 构建管线的输出目录
		/// </summary>
		public string PipelineOutputDirectory { private set; get; }


		public BuildParametersContext(BuildParameters parameters)
		{
			Parameters = parameters;

			PipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(parameters.OutputRoot, parameters.BuildTarget);
			if (parameters.BuildMode == EBuildMode.DryRunBuild)
				PipelineOutputDirectory += $"_{EBuildMode.DryRunBuild}";
			else if (parameters.BuildMode == EBuildMode.SimulateBuild)
				PipelineOutputDirectory += $"_{EBuildMode.SimulateBuild}";
		}

		/// <summary>
		/// 获取本次构建的补丁目录
		/// </summary>
		public string GetPackageDirectory()
		{
			return $"{Parameters.OutputRoot}/{Parameters.BuildTarget}/{Parameters.BuildVersion}";
		}

		/// <summary>
		/// 获取构建选项
		/// </summary>
		public BuildAssetBundleOptions GetPipelineBuildOptions()
		{
			// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
			// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

			BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
			opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

			if (Parameters.BuildMode == EBuildMode.SimulateBuild)
				throw new Exception("Should never get here !");

			if (Parameters.BuildMode == EBuildMode.DryRunBuild)
			{
				opt |= BuildAssetBundleOptions.DryRunBuild;
				return opt;
			}

			if (Parameters.CompressOption == ECompressOption.Uncompressed)
				opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
			else if (Parameters.CompressOption == ECompressOption.LZ4)
				opt |= BuildAssetBundleOptions.ChunkBasedCompression;

			if (Parameters.BuildMode == EBuildMode.ForceRebuild)
				opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
			if (Parameters.DisableWriteTypeTree)
				opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
			if (Parameters.IgnoreTypeTreeChanges)
				opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

			opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName; //Disables Asset Bundle LoadAsset by file name.
			opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.			

			return opt;
		}

		/// <summary>
		/// 获取构建的耗时（单位：秒）
		/// </summary>
		public float GetBuildingSeconds()
		{
			float seconds = _buildWatch.ElapsedMilliseconds / 1000f;
			return seconds;
		}
		public void BeginWatch()
		{
			_buildWatch.Start();
		}
		public void StopWatch()
		{
			_buildWatch.Stop();
		}
	}
}