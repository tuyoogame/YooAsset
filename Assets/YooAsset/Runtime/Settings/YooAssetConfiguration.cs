using System;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 提供 YooAsset 全局配置的访问入口
    /// </summary>
    public static class YooAssetConfiguration
    {
#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            if (s_settings != null)
            {
                if (UnityEditor.AssetDatabase.Contains(s_settings) == false)
                    UnityEngine.Object.DestroyImmediate(s_settings);
                s_settings = null;
            }
        }
#endif

        private static YooAssetSettings s_settings = null;

        /// <summary>
        /// 获取全局配置实例
        /// </summary>
        /// <returns>全局配置实例</returns>
        /// <remarks>首次调用时从 Resources 加载，加载失败则创建默认实例</remarks>
        internal static YooAssetSettings GetSettings()
        {
            if (s_settings == null)
            {
                s_settings = Resources.Load<YooAssetSettings>("YooAssetSettings");
                if (s_settings == null)
                {
                    YooLogger.Log("YooAssetSettings not found, using default configuration.");
                    s_settings = ScriptableObject.CreateInstance<YooAssetSettings>();
                }
                else
                {
                    YooLogger.Log("YooAssetSettings loaded successfully.");
                }
            }
            return s_settings;
        }


        /// <summary>
        /// 获取资源包裹的根文件夹名称
        /// </summary>
        /// <returns>文件夹名称。如果未配置则返回默认值 "yoo"。</returns>
        public static string GetYooFolderName()
        {
            return GetSettings().YooFolderName;
        }

        /// <summary>
        /// 获取构建报告的文件名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageVersion">包裹版本号</param>
        /// <returns>包含 .report 扩展名的文件名</returns>
        public static string GetBuildReportFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));
            if (string.IsNullOrEmpty(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.PackageFilePrefix))
                return $"{packageName}_{packageVersion}.report";
            else
                return $"{settings.PackageFilePrefix}_{packageName}_{packageVersion}.report";
        }

        /// <summary>
        /// 获取清单二进制文件的文件名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageVersion">包裹版本号</param>
        /// <returns>包含 .bytes 扩展名的文件名</returns>
        public static string GetManifestBinaryFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));
            if (string.IsNullOrEmpty(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.PackageFilePrefix))
                return $"{packageName}_{packageVersion}.bytes";
            else
                return $"{settings.PackageFilePrefix}_{packageName}_{packageVersion}.bytes";
        }

        /// <summary>
        /// 获取清单 JSON 文件的文件名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageVersion">包裹版本号</param>
        /// <returns>包含 .json 扩展名的文件名</returns>
        public static string GetManifestJsonFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));
            if (string.IsNullOrEmpty(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.PackageFilePrefix))
                return $"{packageName}_{packageVersion}.json";
            else
                return $"{settings.PackageFilePrefix}_{packageName}_{packageVersion}.json";
        }

        /// <summary>
        /// 获取包裹的哈希校验文件名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="packageVersion">包裹版本号</param>
        /// <returns>包含 .hash 扩展名的文件名</returns>
        public static string GetPackageHashFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));
            if (string.IsNullOrEmpty(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.PackageFilePrefix))
                return $"{packageName}_{packageVersion}.hash";
            else
                return $"{settings.PackageFilePrefix}_{packageName}_{packageVersion}.hash";
        }

        /// <summary>
        /// 获取包裹的版本记录文件名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>包含 .version 扩展名的文件名</returns>
        public static string GetPackageVersionFileName(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));

            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.PackageFilePrefix))
                return $"{packageName}.version";
            else
                return $"{settings.PackageFilePrefix}_{packageName}.version";
        }

        #region 路径相关
        /// <summary>
        /// 获取编辑器下缓存文件根目录
        /// </summary>
        /// <returns>缓存文件根目录的绝对路径</returns>
        internal static string GetEditorCacheRoot()
        {
            // 注意：为了方便调试查看，编辑器下把存储目录放到项目的 Library 目录下。
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectPath))
                throw new InvalidOperationException("Could not determine project root path from Application.dataPath.");
            projectPath = PathUtility.NormalizePath(projectPath);

            string libraryPath = PathUtility.Combine(projectPath, "Library");
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.YooFolderName))
                return libraryPath;
            else
                return PathUtility.Combine(libraryPath, settings.YooFolderName);
        }

        /// <summary>
        /// 获取PC平台缓存文件根目录
        /// </summary>
        /// <returns>缓存文件根目录的绝对路径</returns>
        internal static string GetWindowsCacheRoot()
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.YooFolderName))
                return Application.dataPath;
            else
                return PathUtility.Combine(Application.dataPath, settings.YooFolderName);
        }

        /// <summary>
        /// 获取Linux平台缓存文件根目录
        /// </summary>
        /// <returns>缓存文件根目录的绝对路径</returns>
        internal static string GetLinuxCacheRoot()
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.YooFolderName))
                return Application.dataPath;
            else
                return PathUtility.Combine(Application.dataPath, settings.YooFolderName);
        }

        /// <summary>
        /// 获取Mac平台缓存文件根目录
        /// </summary>
        /// <returns>缓存文件根目录的绝对路径</returns>
        internal static string GetMacCacheRoot()
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.YooFolderName))
                return Application.persistentDataPath;
            else
                return PathUtility.Combine(Application.persistentDataPath, settings.YooFolderName);
        }

        /// <summary>
        /// 获取移动平台缓存文件根目录
        /// </summary>
        /// <returns>缓存文件根目录的绝对路径</returns>
        internal static string GetMobileCacheRoot()
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.YooFolderName))
                return Application.persistentDataPath;
            else
                return PathUtility.Combine(Application.persistentDataPath, settings.YooFolderName);
        }

        /// <summary>
        /// 获取默认的缓存文件根目录
        /// </summary>
        /// <returns>缓存文件根目录的绝对路径</returns>
        internal static string GetDefaultCacheRoot()
        {
#if UNITY_EDITOR
            return GetEditorCacheRoot();
#elif UNITY_STANDALONE_WIN
            return GetWindowsCacheRoot();
#elif UNITY_STANDALONE_LINUX
            return GetLinuxCacheRoot();
#elif UNITY_STANDALONE_OSX
            return GetMacCacheRoot();
#else
            return GetMobileCacheRoot();
#endif
        }

        /// <summary>
        /// 获取默认的内置文件根目录
        /// </summary>
        /// <returns>内置文件根目录的绝对路径</returns>
        internal static string GetDefaultBuiltinRoot()
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.YooFolderName))
                return Application.streamingAssetsPath;
            else
                return PathUtility.Combine(Application.streamingAssetsPath, settings.YooFolderName);
        }
        #endregion
    }
}
