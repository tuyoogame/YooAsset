using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线，直接拷贝原始文件而不打包为 AssetBundle
    /// </summary>
    public class RawFileBuildPipeline : IBuildPipeline
    {
        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <returns>构建结果</returns>
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is RawFileBuildParameters)
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
                    new TaskPrepare_RFBP(),
                    new TaskGetBuildMap_RFBP(),
                    new TaskBuilding_RFBP(),
                    new TaskEncryption_RFBP(),
                    new TaskUpdateBundleInfo_RFBP(),
                    new TaskCreateManifest_RFBP(),
                    new TaskCreateReport_RFBP(),
                    new TaskCreatePackage_RFBP(),
                    new TaskCopyBundledFiles_RFBP(),
                    new TaskCreateCatalog_RFBP()
                };
            return pipeline;
        }
    }
}