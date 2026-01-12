using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线，使用 Scriptable Build Pipeline 进行构建
    /// </summary>
    public class ScriptableBuildPipeline : IBuildPipeline
    {
        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <returns>构建结果</returns>
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is ScriptableBuildParameters)
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
                    new TaskPrepare_SBP(),
                    new TaskGetBuildMap_SBP(),
                    new TaskBuilding_SBP(),
                    new TaskVerifyBuildResult_SBP(),
                    new TaskEncryption_SBP(),
                    new TaskUpdateBundleInfo_SBP(),
                    new TaskCreateManifest_SBP(),
                    new TaskCreateReport_SBP(),
                    new TaskCreatePackage_SBP(),
                    new TaskCopyBundledFiles_SBP(),
                    new TaskCreateCatalog_SBP()
                };
            return pipeline;
        }
    }
}