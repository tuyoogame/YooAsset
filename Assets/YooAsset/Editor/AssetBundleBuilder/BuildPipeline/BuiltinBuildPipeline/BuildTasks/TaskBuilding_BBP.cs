using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskBuilding_BBP : IBuildTask
    {
        public class BuildResultContext : IContextObject
        {
            public AssetBundleManifest UnityManifest;
        }

        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var builtinBuildParameters = buildParametersContext.Parameters as BuiltinBuildParameters;

            // 开始构建
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            BuildAssetBundleOptions buildOptions = builtinBuildParameters.GetBundleBuildOptions();
            var bundleBuilds = buildMapContext.GetPipelineBuilds(builtinBuildParameters.ReplaceAssetPathWithAddress);
            AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(pipelineOutputDirectory, bundleBuilds, buildOptions, buildParametersContext.Parameters.BuildTarget);
            if (unityManifest == null)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, "UnityEngine build failed !");
                throw new Exception(message);
            }

            // 检测输出目录
            string unityOutputManifestFilePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
            if (System.IO.File.Exists(unityOutputManifestFilePath) == false)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFatal, $"Not found output {nameof(AssetBundleManifest)} file : {unityOutputManifestFilePath}");
                throw new Exception(message);
            }

            BuildLogger.Log("UnityEngine build success !");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.UnityManifest = unityManifest;
            context.SetContextObject(buildResultContext);
        }
    }
}