using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建管线接口，定义构建流程的标准入口
    /// </summary>
    public interface IBuildPipeline
    {
        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <returns>构建结果</returns>
        BuildResult Run(BuildParameters buildParameters, bool enableLog);
    }
}