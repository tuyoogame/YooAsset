using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建任务运行器，按顺序执行构建管线中的所有任务
    /// </summary>
    public static class BuildRunner
    {
        private static Stopwatch s_buildWatch;

        /// <summary>
        /// 构建流程的总耗时（秒）
        /// </summary>
        public static int TotalSeconds { get; private set; } = 0;

        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <param name="pipeline">构建任务列表</param>
        /// <param name="context">构建上下文</param>
        /// <returns>构建结果，包含成功状态和错误信息</returns>
        public static BuildResult Run(List<IBuildTask> pipeline, BuildContext context)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            BuildResult buildResult = new BuildResult();
            buildResult.Success = true;
            TotalSeconds = 0;
            for (int i = 0; i < pipeline.Count; i++)
            {
                IBuildTask task = pipeline[i];
                try
                {
                    s_buildWatch = Stopwatch.StartNew();
                    string taskName = task.GetType().Name.Split('_')[0];
                    BuildLogger.Log($"--------------------------------------------->{taskName}<--------------------------------------------");
                    task.Run(context);
                    s_buildWatch.Stop();

                    // 统计耗时
                    int seconds = GetBuildSeconds();
                    TotalSeconds += seconds;
                    BuildLogger.Log($"{taskName} completed in {seconds} seconds.");
                }
                catch (Exception e)
                {
                    EditorDialogUtility.ClearProgressBar();
                    buildResult.FailedTask = task.GetType().Name;
                    buildResult.ErrorInfo = e.ToString();
                    buildResult.ErrorStack = e.StackTrace;
                    buildResult.Success = false;
                    break;
                }
            }

            // 返回运行结果
            BuildLogger.Log($"Total build process time: {TotalSeconds} seconds.");
            return buildResult;
        }

        private static int GetBuildSeconds()
        {
            float seconds = s_buildWatch.ElapsedMilliseconds / 1000f;
            return (int)seconds;
        }
    }
}