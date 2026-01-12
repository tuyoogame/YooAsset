using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    internal class ClearBuildCacheWindow
    {
        [MenuItem("Tools/Clear Build Cache", false, 2)]
        public static void OpenWindow()
        {
            // 清空SBP构建缓存
            UnityEditor.Build.Pipeline.Utilities.BuildCache.PurgeCache(false);

            // 删除AssetDependDB文件
            string projectPath = YooAsset.Editor.EditorPathUtility.GetProjectPath();
            string databaseFilePath = $"{projectPath}/Library/AssetDependencyDB";
            if (File.Exists(databaseFilePath))
            {
                File.Delete(databaseFilePath);
            }
        }
    }
}