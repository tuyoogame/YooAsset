using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线，使用 Unity 内置的 AssetBundle 构建接口
    /// </summary>
    public class LegacyBuildPipeline : IBuildPipeline
    {
        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <returns>构建结果</returns>
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is LegacyBuildParameters)
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
                    new TaskPrepare_LBP(),
                    new TaskGetBuildMap_LBP(),
                    new TaskBuilding_LBP(),
                    new TaskVerifyBuildResult_LBP(),
                    new TaskEncryption_LBP(),
                    new TaskUpdateBundleInfo_LBP(),
                    new TaskCreateManifest_LBP(),
                    new TaskCreateReport_LBP(),
                    new TaskCreatePackage_LBP(),
                    new TaskCopyBundledFiles_LBP(),
                    new TaskCreateCatalog_LBP()
                };
            return pipeline;
        }
    }
}