using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包构建辅助工具类
    /// </summary>
    public static class BundleBuilderHelper
    {
        /// <summary>
        /// 获取默认的输出根目录
        /// </summary>
        /// <returns>默认的构建输出根目录路径</returns>
        public static string GetDefaultBuildOutputRoot()
        {
            string projectPath = EditorPathUtility.GetProjectPath();
            return $"{projectPath}/Bundles";
        }

        /// <summary>
        /// 获取 StreamingAssets 根目录路径
        /// </summary>
        /// <returns>StreamingAssets 根目录路径</returns>
        public static string GetStreamingAssetsRoot()
        {
            return YooAssetConfiguration.GetDefaultBuiltinRoot();
        }
    }
}