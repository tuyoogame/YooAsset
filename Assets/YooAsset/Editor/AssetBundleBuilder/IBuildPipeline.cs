using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public interface IBuildPipeline
    {
        /// <summary>
        /// 运行构建任务
        /// </summary>
        BuildResult Run(BuildParameters buildParameters, bool enableLog);
    }
}