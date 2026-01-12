
using System.Collections.Generic;
using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器模拟构建管线，用于编辑器下模拟资源加载
    /// </summary>
    public class EditorSimulateBuildPipeline : IBuildPipeline
    {
        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <returns>构建结果</returns>
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is EditorSimulateBuildParameters)
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
                    new TaskPrepare_ESBP(),
                    new TaskGetBuildMap_ESBP(),
                    new TaskUpdateBundleInfo_ESBP(),
                    new TaskCreateManifest_ESBP()
                };
            return pipeline;
        }
    }
}