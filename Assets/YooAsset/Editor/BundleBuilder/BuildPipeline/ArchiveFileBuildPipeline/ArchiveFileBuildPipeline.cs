using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线，将同一 BundleName 下的多个原始文件合并写入一个归档文件
    /// </summary>
    public class ArchiveFileBuildPipeline : IBuildPipeline
    {
        /// <summary>
        /// 执行构建流程
        /// </summary>
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is ArchiveFileBuildParameters)
            {
                BundleBuilder builder = new BundleBuilder();
                return builder.Run(buildParameters, GetDefaultBuildPipeline(), enableLog);
            }
            else
            {
                throw new ArgumentException($"Invalid build parameter type: '{buildParameters.GetType().Name}'.", nameof(buildParameters));
            }
        }

        /// <summary>
        /// 获取默认的构建流程
        /// </summary>
        private List<IBuildTask> GetDefaultBuildPipeline()
        {
            List<IBuildTask> pipeline = new List<IBuildTask>
            {
                new TaskPrepare_AFBP(),
                new TaskGetBuildMap_AFBP(),
                new TaskBuilding_AFBP(),
                new TaskEncryption_AFBP(),
                new TaskUpdateBundleInfo_AFBP(),
                new TaskCreateManifest_AFBP(),
                new TaskCreateReport_AFBP(),
                new TaskCreatePackage_AFBP(),
                new TaskCopyBundledFiles_AFBP(),
                new TaskCreateCatalog_AFBP()
            };
            return pipeline;
        }
    }
}
