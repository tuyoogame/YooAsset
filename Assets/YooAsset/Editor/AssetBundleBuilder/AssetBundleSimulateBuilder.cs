using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public static class AssetBundleSimulateBuilder
    {
        /// <summary>
        /// 模拟构建
        /// </summary>
        public static PackageInvokeBuildResult SimulateBuild(PackageInvokeBuildParam buildParam)
        {
            string packageName = buildParam.PackageName;
            string buildPipelineName = buildParam.BuildPipelineName;

            if (buildPipelineName == "EditorSimulateBuildPipeline")
            {
                var buildParameters = new EditorSimulateBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuiltinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = EBuildPipeline.EditorSimulateBuildPipeline.ToString();
                buildParameters.BuildBundleType = (int)EBuildBundleType.VirtualBundle;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = "Simulate";
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuiltinFileCopyOption = EBuiltinFileCopyOption.None;
                buildParameters.BuiltinFileCopyParams = string.Empty;
                buildParameters.UseAssetDependencyDB = true;

                var pipeline = new EditorSimulateBuildPipeline();
                BuildResult buildResult = pipeline.Run(buildParameters, false);
                if (buildResult.Success)
                {
                    var reulst = new PackageInvokeBuildResult();
                    reulst.PackageRootDirectory = buildResult.OutputPackageDirectory;
                    return reulst;
                }
                else
                {
                    Debug.LogError(buildResult.ErrorInfo);
                    throw new System.Exception($"{nameof(EditorSimulateBuildPipeline)} build failed !");
                }
            }
            else
            {
                throw new System.NotImplementedException(buildPipelineName);
            }
        }
    }
}