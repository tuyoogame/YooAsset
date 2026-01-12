using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的清单文件创建任务
    /// </summary>
    public class TaskCreateManifest_SBP : TaskCreateManifest, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;
            bool replaceAssetPathWithAddress = scriptableBuildParameters.ReplaceAssetPathWithAddress;
            CreateManifestFile(true, true, replaceAssetPathWithAddress, context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();

            if (buildResultContext.Results.BundleInfos.ContainsKey(bundleName) == false)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleInBuildResult, $"Bundle not found in engine build result: '{bundleName}'.");
                throw new InvalidOperationException(message);
            }
            return buildResultContext.Results.BundleInfos[bundleName].Dependencies;
        }
    }
}