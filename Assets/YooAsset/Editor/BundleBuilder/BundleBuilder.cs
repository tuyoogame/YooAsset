using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包构建器，负责执行完整的资源包构建流程
    /// </summary>
    public class BundleBuilder
    {
        private readonly BuildContext _buildContext = new BuildContext();

        /// <summary>
        /// 执行资源包构建流程
        /// </summary>
        /// <param name="buildParameters">构建参数</param>
        /// <param name="buildPipeline">构建任务列表</param>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <returns>构建结果</returns>
        public BuildResult Run(BuildParameters buildParameters, List<IBuildTask> buildPipeline, bool enableLog)
        {
            // 检测构建参数是否为空
            if (buildParameters == null)
                throw new ArgumentNullException(nameof(buildParameters));

            // 检测构建管线是否为空
            if (buildPipeline == null)
                throw new ArgumentNullException(nameof(buildPipeline));
            if (buildPipeline.Count == 0)
                throw new ArgumentException("Build pipeline task list is empty.", nameof(buildPipeline));

            // 清空旧数据
            _buildContext.ClearAllContext();

            // 构建参数
            var buildParametersContext = new BuildParametersContext(buildParameters);
            _buildContext.SetContextObject(buildParametersContext);

            // 初始化日志系统
            string logFilePath = $"{buildParametersContext.GetPipelineOutputDirectory()}/buildInfo.log";
            BuildLogger.InitLogger(enableLog, logFilePath);

            // 执行构建流程
            BuildLogger.Log($"Building package '{buildParameters.PackageName}' with pipeline '{buildParameters.BuildPipeline}'.");
            var buildResult = BuildRunner.Run(buildPipeline, _buildContext);
            if (buildResult.Success)
            {
                buildResult.OutputPackageDirectory = buildParametersContext.GetPackageOutputDirectory();
                BuildLogger.Log("Resource pipeline build succeeded.");
            }
            else
            {
                BuildLogger.Error($"Build pipeline '{buildParameters.BuildPipeline}' build failed.");
                BuildLogger.Error($"Error occurred in build task '{buildResult.FailedTask}'.");
                BuildLogger.Error(buildResult.ErrorInfo);
            }

            // 关闭日志系统
            BuildLogger.Shutdown();

            return buildResult;
        }
    }
}