using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的清单文件创建任务
    /// </summary>
    public class TaskCreateManifest_LBP : TaskCreateManifest, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var legacyBuildParameters = buildParametersContext.Parameters as LegacyBuildParameters;
            bool replaceAssetPathWithAddress = legacyBuildParameters.ReplaceAssetPathWithAddress;
            CreateManifestFile(true, true, replaceAssetPathWithAddress, context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            var buildResultContext = context.GetContextObject<TaskBuilding_LBP.BuildResultContext>();
            return buildResultContext.UnityManifest.GetAllDependencies(bundleName);
        }
    }
}