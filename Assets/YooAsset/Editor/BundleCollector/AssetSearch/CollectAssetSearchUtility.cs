using System;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 收集资源搜索工具类
    /// </summary>
    public static class CollectAssetSearchUtility
    {
        /// <summary>
        /// 验证搜索路径
        /// </summary>
        /// <param name="input">搜索路径</param>
        /// <returns>搜索路径错误类型</returns>
        public static ECollectAssetSearchError ValidateSearchPath(string input)
        {
            if (string.IsNullOrEmpty(input))
                return ECollectAssetSearchError.InputPathEmpty;

            if (input.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) == false)
                return ECollectAssetSearchError.InputPathMissingAssetsPrefix;

            if (input.EndsWith("/"))
                return ECollectAssetSearchError.InputPathEndsWithSlash;

            string fileName = System.IO.Path.GetFileName(input);
            if (fileName.Contains(".") == false)
                return ECollectAssetSearchError.InputPathMissingExtension;

            if (AssetDatabase.IsValidFolder(input))
                return ECollectAssetSearchError.InputPathIsFolder;

            string guid = AssetDatabase.AssetPathToGUID(input);
            if (string.IsNullOrEmpty(guid))
                return ECollectAssetSearchError.AssetPathNotExists;

            return ECollectAssetSearchError.None;
        }

        /// <summary>
        /// 获取搜索路径错误提示信息
        /// </summary>
        /// <param name="error">搜索路径错误类型</param>
        /// <param name="input">搜索路径</param>
        /// <returns>错误提示信息</returns>
        public static string GetSearchPathErrorMessage(ECollectAssetSearchError error, string input)
        {
            switch (error)
            {
                case ECollectAssetSearchError.InputPathEmpty:
                    return "Please enter an asset path.";
                case ECollectAssetSearchError.InputPathMissingAssetsPrefix:
                    return "Path must start with Assets/.";
                case ECollectAssetSearchError.InputPathEndsWithSlash:
                    return "Please enter a file path. Do not end with /.";
                case ECollectAssetSearchError.InputPathMissingExtension:
                    return "Path is missing a file extension (e.g. .prefab, .png, .mat).";
                case ECollectAssetSearchError.InputPathIsFolder:
                    return "Please enter an asset file path, not a folder path.";
                case ECollectAssetSearchError.AssetPathNotExists:
                    return $"Asset not found: {input}";
                default:
                    return "Invalid input format.";
            }
        }

        /// <summary>
        /// 在指定 Package 中搜索资源路径，找到第一个命中结果即返回
        /// </summary>
        /// <param name="package">搜索的资源包裹</param>
        /// <param name="assetPath">资源路径</param>
        /// <returns>搜索结果，如果未找到返回 null</returns>
        public static CollectAssetSearchResult SearchAssetPath(BundleCollectorPackage package, string assetPath)
        {
            if (ValidateSearchPath(assetPath) != ECollectAssetSearchError.None)
                return null;

            IAssetIgnoreRule ignoreRule = BundleCollectorSettingData.GetAssetIgnoreRuleInstance(package.IgnoreRuleName);
            var command = new CollectCommand(package.PackageName, ignoreRule);
            command.SetFlag(ECollectFlags.IgnoreGetDependencies, true);
            command.UniqueBundleName = BundleCollectorSettingData.Setting.UniqueBundleName;
            command.EnableAddressable = package.EnableAddressable;
            command.SupportExtensionless = package.SupportExtensionless;
            command.LocationToLower = package.LocationToLower;
            command.IncludeAssetGUID = package.IncludeAssetGUID;
            command.AutoCollectShaders = package.AutoCollectShaders;

            for (int groupIndex = 0; groupIndex < package.Groups.Count; groupIndex++)
            {
                var group = package.Groups[groupIndex];
                for (int collectIndex = 0; collectIndex < group.Collectors.Count; collectIndex++)
                {
                    var collector = group.Collectors[collectIndex];

                    // 判断收集器是否可能收集指定资源
                    if (IsCandidateCollector(collector, assetPath) == false)
                        continue;

                    try
                    {
                        // 检测配置是否有效
                        collector.CheckConfigError();

                        // 收集有效资源信息
                        var collectAssets = collector.GetAllCollectAssets(command, group);
                        foreach (var collectAsset in collectAssets)
                        {
                            if (string.Equals(collectAsset.AssetInfo.AssetPath, assetPath, StringComparison.OrdinalIgnoreCase))
                            {
                                return new CollectAssetSearchResult(group, groupIndex, collector, collectIndex, assetPath);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Invalid collector : {collector.CollectPath}, error: {e.Message}");
                    }
                }
            }

            // 未找到匹配资源
            return null;
        }

        /// <summary>
        /// 判断收集器是否可能收集指定资源
        /// </summary>
        /// <param name="collector">收集器</param>
        /// <param name="assetPath">资源路径</param>
        /// <returns>如果收集器可能收集该资源返回 true</returns>
        private static bool IsCandidateCollector(BundleCollector collector, string assetPath)
        {
            if (string.IsNullOrEmpty(collector.CollectPath))
                return false;

            if (AssetDatabase.IsValidFolder(collector.CollectPath))
            {
                string folderPath = collector.CollectPath.TrimEnd('/') + "/";
                return assetPath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase);
            }

            // 注意：资源收集器也可能直接配置的单个资源路径
            return string.Equals(assetPath, collector.CollectPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
