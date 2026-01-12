using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建管线查看器的标识特性，用于关联查看器与管线名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BuildPipelineAttribute : Attribute
    {
        /// <summary>
        /// 关联的构建管线名称
        /// </summary>
        public string PipelineName { get; }

        /// <summary>
        /// 创建 BuildPipelineAttribute 实例
        /// </summary>
        /// <param name="pipelineName">构建管线名称</param>
        public BuildPipelineAttribute(string pipelineName)
        {
            PipelineName = pipelineName;
        }
    }
}
