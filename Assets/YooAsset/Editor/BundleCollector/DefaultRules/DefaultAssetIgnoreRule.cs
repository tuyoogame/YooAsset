using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 默认忽略规则的工具类
    /// </summary>
    public class DefaultAssetIgnoreRule
    {
        private static readonly HashSet<string> s_ignoreFileExtensions = new HashSet<string>() 
        { "", ".so", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl" };

        /// <summary>
        /// 查询文件扩展名是否在忽略列表中
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>如果在忽略列表中返回 true</returns>
        public static bool IsIgnoreFileExtension(string extension)
        {
            return s_ignoreFileExtensions.Contains(extension);
        }
    }

    /// <summary>
    /// 适配常规的资源构建管线
    /// </summary>
    public class NormalIgnoreRule : IAssetIgnoreRule
    {
        /// <inheritdoc/>
        public bool IsIgnoreAsset(EditorAssetInfo assetInfo)
        {
            if (assetInfo.AssetPath.StartsWith("Assets/") == false && assetInfo.AssetPath.StartsWith("Packages/") == false)
            {
                UnityEngine.Debug.LogError($"Asset path is invalid: '{assetInfo.AssetPath}'.");
                return true;
            }

            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetInfo.AssetPath))
                return true;

            // 忽略编辑器图标资源
            if (assetInfo.AssetPath.Contains("/Gizmos/"))
                return true;

            // 忽略编辑器专属资源
            if (assetInfo.AssetPath.Contains("/Editor/") || assetInfo.AssetPath.Contains("/Editor Resources/"))
                return true;

            // 忽略编辑器下的类型资源
            if (assetInfo.AssetType == typeof(LightingDataAsset))
                return true;
            if (assetInfo.AssetType == typeof(LightmapParameters))
                return true;

            // 忽略Unity引擎无法识别的文件
            if (assetInfo.AssetType == typeof(UnityEditor.DefaultAsset))
            {
                UnityEngine.Debug.LogWarning($"Default asset cannot be packed: '{assetInfo.AssetPath}'.");
                return true;
            }

            return DefaultAssetIgnoreRule.IsIgnoreFileExtension(assetInfo.FileExtension);
        }
    }

    /// <summary>
    /// 适配原生文件构建管线
    /// </summary>
    public class RawFileIgnoreRule : IAssetIgnoreRule
    {
        /// <inheritdoc/>
        public bool IsIgnoreAsset(EditorAssetInfo assetInfo)
        {
            if (assetInfo.AssetPath.StartsWith("Assets/") == false && assetInfo.AssetPath.StartsWith("Packages/") == false)
            {
                UnityEngine.Debug.LogError($"Asset path is invalid: '{assetInfo.AssetPath}'.");
                return true;
            }

            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetInfo.AssetPath))
                return true;

            // 忽略编辑器下的类型资源
            if (assetInfo.AssetType == typeof(LightingDataAsset))
                return true;
            if (assetInfo.AssetType == typeof(LightmapParameters))
                return true;

            return DefaultAssetIgnoreRule.IsIgnoreFileExtension(assetInfo.FileExtension);
        }
    }
}
