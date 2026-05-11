using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建准备任务的基类，提供各管线共用的准备阶段方法。
    /// </summary>
    public class TaskPrepare
    {
        /// <summary>
        /// 检测是否有未保存的场景
        /// </summary>
        protected void CheckDirtyScenes()
        {
            if (EditorSceneUtility.HasDirtyScenes())
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.FoundUnsavedScene, "Found unsaved scene.");
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// 删除包裹根目录
        /// </summary>
        protected void DeletePackageRootDirectory(BuildParameters buildParameters)
        {
            string packageRootDirectory = buildParameters.GetPackageRootDirectory();
            if (EditorFileUtility.DeleteDirectory(packageRootDirectory))
            {
                BuildLogger.Log($"Delete package root directory: '{packageRootDirectory}'.");
            }
        }

        /// <summary>
        /// 检测包裹输出目录是否已存在，并创建管线输出目录
        /// </summary>
        protected void PrepareOutputDirectory(BuildParameters buildParameters)
        {
            // 如果包裹输出目录已经存在
            string packageOutputDirectory = buildParameters.GetPackageOutputDirectory();
            if (Directory.Exists(packageOutputDirectory))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackageOutputDirectoryExists, $"Package output directory exists: '{packageOutputDirectory}'.");
                throw new InvalidOperationException(message);
            }

            // 创建包裹输出目录
            string pipelineOutputDirectory = buildParameters.GetPipelineOutputDirectory();
            if (EditorFileUtility.CreateDirectory(pipelineOutputDirectory))
            {
                BuildLogger.Log($"Create pipeline output directory: '{pipelineOutputDirectory}'.");
            }
        }
    }
}
