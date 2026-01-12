using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建参数上下文
    /// </summary>
    [ContextObject]
    public class BuildParametersContext
    {
        /// <summary>
        /// 构建参数
        /// </summary>
        public BuildParameters Parameters { private set; get; }


        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="parameters">构建参数</param>
        public BuildParametersContext(BuildParameters parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// 检测构建参数是否合法
        /// </summary>
        public void CheckBuildParameters()
        {
            Parameters.CheckBuildParameters();
        }

        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        /// <returns>构建管线的输出目录路径</returns>
        public string GetPipelineOutputDirectory()
        {
            return Parameters.GetPipelineOutputDirectory();
        }

        /// <summary>
        /// 获取本次构建的补丁输出目录
        /// </summary>
        /// <returns>本次构建的补丁输出目录路径</returns>
        public string GetPackageOutputDirectory()
        {
            return Parameters.GetPackageOutputDirectory();
        }

        /// <summary>
        /// 获取本次构建的补丁根目录
        /// </summary>
        /// <returns>本次构建的补丁根目录路径</returns>
        public string GetPackageRootDirectory()
        {
            return Parameters.GetPackageRootDirectory();
        }

        /// <summary>
        /// 获取首包资源的根目录
        /// </summary>
        /// <returns>首包资源的根目录路径</returns>
        public string GetBundledRootDirectory()
        {
            return Parameters.GetBundledRootDirectory();
        }
    }
}