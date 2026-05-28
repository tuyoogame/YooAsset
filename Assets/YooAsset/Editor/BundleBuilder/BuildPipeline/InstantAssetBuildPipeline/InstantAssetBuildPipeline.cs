#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结引擎构建管线
    /// </summary>
    public class InstantAssetBuildPipeline : IBuildPipeline
    {
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is InstantAssetBuildParameters)
            {
                BundleBuilder builder = new BundleBuilder();
                return builder.Run(buildParameters, GetDefaultBuildPipeline(), enableLog);
            }
            else
            {
                throw new ArgumentException($"Invalid build parameter type: '{buildParameters.GetType().Name}'.", nameof(buildParameters));
            }
        }

        private List<IBuildTask> GetDefaultBuildPipeline()
        {
            List<IBuildTask> pipeline = new List<IBuildTask>
            {
                new TaskPrepare_IABP(),
                new TaskGetBuildMap_IABP(),
                new TaskCreateRecordFile_IABP(),
                new TaskBuilding_IABP(),
                //new TaskVerifyBuildResult_IABP(),
                //new TaskUpdateBundleInfo_IABP(),
                //new TaskCreateManifest_IABP(),
                //new TaskCreateReport_IABP(),
                //new TaskCreatePackage_IABP(),
                //new TaskCopyBundledFiles_IABP(),
            };
            return pipeline;
        }
    }
}
#endif
