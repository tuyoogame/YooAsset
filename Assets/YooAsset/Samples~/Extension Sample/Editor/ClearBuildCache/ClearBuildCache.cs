using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 提供构建缓存清理菜单入口
    /// </summary>
    internal class ClearBuildCacheWindow
    {
        private const string SBPEditorAssemblyName = "Unity.ScriptableBuildPipeline.Editor";
        private const string SBPBuildCacheTypeName = "UnityEditor.Build.Pipeline.Utilities.BuildCache";

        [MenuItem("Tools/Clear Build Cache", false, 2)]
        public static void OpenWindow()
        {
            // 清空SBP构建缓存
            var buildCacheType = Type.GetType($"{SBPBuildCacheTypeName}, {SBPEditorAssemblyName}");
            if (buildCacheType != null)
            {
                EditorAssemblyUtility.InvokePublicStaticMethod(buildCacheType, "PurgeCache", false);
            }
            else
            {
                Debug.LogWarning($"Failed to find type: {SBPBuildCacheTypeName}");
            }

            // 删除AssetDependDB文件
            string projectPath = YooAsset.Editor.EditorPathUtility.GetProjectPath();
            string databaseFilePath = $"{projectPath}/Library/AssetDependencyDB";
            if (File.Exists(databaseFilePath))
            {
                File.Delete(databaseFilePath);
            }

            Debug.Log("Clear build cache succeeded !");
        }
    }
}