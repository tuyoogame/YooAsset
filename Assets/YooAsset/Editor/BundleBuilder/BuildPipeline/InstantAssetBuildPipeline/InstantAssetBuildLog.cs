#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的构建日志数据
    /// </summary>
    [Serializable]
    internal class InstantAssetBuildLog
    {
        #region 类型定义
        /// <summary>
        /// 资源记录，用于描述单个资源关联的分包信息。
        /// </summary>
        [Serializable]
        internal class AssetInfo
        {
            /// <summary>
            /// 资源路径（通常为小写的 AssetDatabase 资源路径）
            /// </summary>
            public string first;

            /// <summary>
            /// 该资源关联的分包列表
            /// </summary>
            public List<AssetPackInfo> second;
        }

        /// <summary>
        /// 资源关联的分包信息，用于描述资源和单个分包的关系。
        /// </summary>
        [Serializable]
        internal class AssetPackInfo
        {
            /// <summary>
            /// 分包名称
            /// </summary>
            public string first;

            /// <summary>
            /// 该资源在当前分包中关联的资源 GUID 列表
            /// </summary>
            public List<string> second;
        }

        /// <summary>
        /// 分包信息，用于描述单个分包的构建结果。
        /// </summary>
        [Serializable]
        internal class BundleInfo
        {
            /// <summary>
            /// 分包名称（也是构建输出目录下的文件名）
            /// </summary>
            public string first;

            /// <summary>
            /// 当前分包的详细信息
            /// </summary>
            public BundleDetail second;
        }

        /// <summary>
        /// 分包细节，用于描述分包的校验值和文件列表。
        /// </summary>
        [Serializable]
        internal class BundleDetail
        {
            /// <summary>
            /// 分包的 CRC 值
            /// </summary>
            public string crc;

            /// <summary>
            /// 当前分包包含的文件列表
            /// </summary>
            public List<FileInfo> files;
        }

        /// <summary>
        /// 文件信息，用于描述分包内的单个文件。
        /// </summary>
        [Serializable]
        internal class FileInfo
        {
            private const string SceneFolderPrefix = "Scene/";

            /// <summary>
            /// 文件在 Instant 构建内部的存储路径
            /// </summary>
            public string libPath;

            /// <summary>
            /// 文件对应的资源路径
            /// </summary>
            public string assetPath;

            /// <summary>
            /// 文件对应的资源 GUID
            /// </summary>
            public string guid;

            /// <summary>
            /// 文件大小，单位为字节
            /// </summary>
            public long size;

            /// <summary>
            /// 是否为场景文件
            /// </summary>
            public bool IsSceneFile()
            {
                return libPath.StartsWith(SceneFolderPrefix, StringComparison.OrdinalIgnoreCase);
            }
        }
        #endregion

        /// <summary>
        /// 构建日志的文件名称
        /// </summary>
        public const string BuildLogFileName = "build_log.json";

        /// <summary>
        /// 内置资源的分包名称
        /// </summary>
        public const string BuiltInExtraResources = "Built-In-Extra-Resources";

        /// <summary>
        /// 构建涉及的资源列表
        /// </summary>
        public List<AssetInfo> assets;

        /// <summary>
        /// 构建生成的分包列表
        /// </summary>
        public List<BundleInfo> bundles;

        [NonSerialized]
        private Dictionary<string, List<string>> _bundleNameToAssetGuids;


        /// <summary>
        /// 初始化查询缓存
        /// </summary>
        public void Initialize()
        {
            if (bundles == null || bundles.Count == 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.InvalidInstantAssetBuildLog, "InstantAsset build log bundles is null or empty.");
                throw new InvalidOperationException(message);
            }

            _bundleNameToAssetGuids = new Dictionary<string, List<string>>(bundles.Count);
            foreach (var bundleInfo in bundles)
            {
                string bundleName = bundleInfo.first;
                if (string.IsNullOrEmpty(bundleName))
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.InvalidInstantAssetBuildLog, "InstantAsset build log bundle name is null or empty.");
                    throw new InvalidOperationException(message);
                }

                if (bundleName == BuiltInExtraResources)
                    continue;

                var assetGuids = new List<string>();
                if (bundleInfo.second != null && bundleInfo.second.files != null)
                {
                    foreach (var fileInfo in bundleInfo.second.files)
                    {
                        if (string.IsNullOrEmpty(fileInfo.guid))
                        {
                            if (fileInfo.IsSceneFile())
                                continue;

                            string message = BuildLogger.GetErrorMessage(ErrorCode.InvalidInstantAssetBuildLog, $"InstantAsset build log file GUID is null or empty: '{bundleName}'.");
                            throw new InvalidOperationException(message);
                        }
                        assetGuids.Add(fileInfo.guid);
                    }
                }
                _bundleNameToAssetGuids[bundleName] = assetGuids;
            }
        }

        /// <summary>
        /// 获取所有分包名称
        /// </summary>
        public List<string> GetAllBundleNames()
        {
            return new List<string>(_bundleNameToAssetGuids.Keys);
        }

        /// <summary>
        /// 尝试获取分包内的资源 GUID 列表
        /// </summary>
        public bool TryGetAssetGuids(string bundleName, out List<string> assetGuids)
        {
            return _bundleNameToAssetGuids.TryGetValue(bundleName, out assetGuids);
        }

        /// <summary>
        /// 从文件加载 InstantAsset 构建日志
        /// </summary>
        public static InstantAssetBuildLog LoadFromFile(string buildLogFilePath)
        {
            if (File.Exists(buildLogFilePath) == false)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.InvalidInstantAssetBuildLog, $"InstantAsset build log not found: '{buildLogFilePath}'.");
                throw new InvalidOperationException(message);
            }

            string jsonData = FileUtility.ReadAllText(buildLogFilePath);
            var buildLog = JsonUtility.FromJson<InstantAssetBuildLog>(jsonData);
            if (buildLog == null)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.InvalidInstantAssetBuildLog, $"InstantAsset build log is invalid: '{buildLogFilePath}'.");
                throw new InvalidOperationException(message);
            }

            buildLog.Initialize();
            return buildLog;
        }
    }
}
#endif
