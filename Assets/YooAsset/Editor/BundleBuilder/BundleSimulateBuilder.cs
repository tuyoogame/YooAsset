using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包模拟构建器，用于编辑器下模拟构建流程
    /// </summary>
    public static class BundleSimulateBuilder
    {
        /// <summary>
        /// 执行模拟构建
        /// </summary>
        /// <param name="buildParam">包裹构建参数</param>
        /// <returns>包裹构建结果</returns>
        public static PackageBuildResult SimulateBuild(PackageBuildParameters buildParam)
        {
            string buildPipelineName = buildParam.BuildPipelineName;
            if (buildPipelineName == EBuildPipeline.EditorSimulateBuildPipeline.ToString())
            {
                var buildParameters = new EditorSimulateBuildParameters();
                buildParameters.BuildOutputRoot = BundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = EBuildPipeline.EditorSimulateBuildPipeline.ToString();
                buildParameters.BuildBundleType = buildParam.BuildBundleType;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.PackageName = buildParam.PackageName;
                buildParameters.PackageVersion = "Simulate";
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BundledCopyOption = EBundledCopyOption.None;
                buildParameters.BundledCopyParams = string.Empty;
                buildParameters.UseAssetDependencyDB = true;

                var pipeline = new EditorSimulateBuildPipeline();
                BuildResult buildResult = pipeline.Run(buildParameters, false);
                if (buildResult.Success)
                {
                    var result = new PackageBuildResult();
                    result.PackageRootDirectory = buildResult.OutputPackageDirectory;
                    return result;
                }
                else
                {
                    Debug.LogError(buildResult.ErrorInfo);
                    throw new InvalidOperationException($"{nameof(EditorSimulateBuildPipeline)} build failed.");
                }
            }
            else
            {
                throw new System.NotImplementedException(buildPipelineName);
            }
        }
    }
}