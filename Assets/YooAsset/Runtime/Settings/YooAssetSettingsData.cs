using System.IO;
using UnityEngine;

namespace YooAsset
{
    public static class YooAssetSettingsData
    {
#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            _setting = null;
        }
#endif

        private static YooAssetSettings _setting = null;
        internal static YooAssetSettings Setting
        {
            get
            {
                if (_setting == null)
                    LoadSettingData();
                return _setting;
            }
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private static void LoadSettingData()
        {
            _setting = Resources.Load<YooAssetSettings>("YooAssetSettings");
            if (_setting == null)
            {
                YooLogger.Log("YooAsset use default settings.");
                _setting = ScriptableObject.CreateInstance<YooAssetSettings>();
            }
            else
            {
                YooLogger.Log("YooAsset use user settings.");
            }
        }


        /// <summary>
        /// 获取YooAsset文件夹名称
        /// </summary>
        public static string GetDefaultYooFolderName()
        {
            return Setting.DefaultYooFolderName;
        }

        /// <summary>
        /// 获取构建报告文件名
        /// </summary>
        public static string GetBuildReportFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(Setting.PackageManifestPrefix))
                return $"{packageName}_{packageVersion}.report";
            else
                return $"{Setting.PackageManifestPrefix}_{packageName}_{packageVersion}.report";
        }

        /// <summary>
        /// 获取清单文件完整名称
        /// </summary>
        public static string GetManifestBinaryFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(Setting.PackageManifestPrefix))
                return $"{packageName}_{packageVersion}.bytes";
            else
                return $"{Setting.PackageManifestPrefix}_{packageName}_{packageVersion}.bytes";
        }

        /// <summary>
        /// 获取清单文件完整名称
        /// </summary>
        public static string GetManifestJsonFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(Setting.PackageManifestPrefix))
                return $"{packageName}_{packageVersion}.json";
            else
                return $"{Setting.PackageManifestPrefix}_{packageName}_{packageVersion}.json";
        }

        /// <summary>
        /// 获取包裹的哈希文件完整名称
        /// </summary>
        public static string GetPackageHashFileName(string packageName, string packageVersion)
        {
            if (string.IsNullOrEmpty(Setting.PackageManifestPrefix))
                return $"{packageName}_{packageVersion}.hash";
            else
                return $"{Setting.PackageManifestPrefix}_{packageName}_{packageVersion}.hash";
        }

        /// <summary>
        /// 获取包裹的版本文件完整名称
        /// </summary>
        public static string GetPackageVersionFileName(string packageName)
        {
            if (string.IsNullOrEmpty(Setting.PackageManifestPrefix))
                return $"{packageName}.version";
            else
                return $"{Setting.PackageManifestPrefix}_{packageName}.version";
        }

        #region 路径相关
        /// <summary>
        /// 获取YOO的Resources目录的加载路径
        /// </summary>
        internal static string GetYooResourcesLoadPath(string packageName, string fileName)
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return PathUtility.Combine(packageName, fileName);
            else
                return PathUtility.Combine(Setting.DefaultYooFolderName, packageName, fileName);
        }

        /// <summary>
        /// 获取YOO的Resources目录的全路径
        /// </summary>
        internal static string GetYooResourcesFullPath()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return $"Assets/Resources";
            else
                return $"Assets/Resources/{Setting.DefaultYooFolderName}";
        }

        /// <summary>
        /// 获取YOO的编辑器下缓存文件根目录
        /// </summary>
        internal static string GetYooEditorCacheRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                projectPath = PathUtility.RegularPath(projectPath);
                return projectPath;
            }
            else
            {
                // 注意：为了方便调试查看，编辑器下把存储目录放到项目根目录下。
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                projectPath = PathUtility.RegularPath(projectPath);
                return PathUtility.Combine(projectPath, Setting.DefaultYooFolderName);
            }
        }

        /// <summary>
        /// 获取YOO的PC平台缓存文件根目录
        /// </summary>
        internal static string GetYooStandaloneWinCacheRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return Application.dataPath;
            else
                return PathUtility.Combine(Application.dataPath, Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 获取YOO的Linux平台缓存文件根目录
        /// </summary>
        internal static string GetYooStandaloneLinuxCacheRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return Application.dataPath;
            else
                return PathUtility.Combine(Application.dataPath, Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 获取YOO的Mac平台缓存文件根目录
        /// </summary>
        internal static string GetYooStandaloneMacCacheRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return Application.persistentDataPath;
            else
                return PathUtility.Combine(Application.persistentDataPath, Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 获取YOO的移动平台缓存文件根目录
        /// </summary>
        internal static string GetYooMobileCacheRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return Application.persistentDataPath;
            else
                return PathUtility.Combine(Application.persistentDataPath, Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 获取YOO默认的缓存文件根目录
        /// </summary>
        internal static string GetYooDefaultCacheRoot()
        {
#if UNITY_EDITOR
            return GetYooEditorCacheRoot();
#elif UNITY_STANDALONE_WIN
            return GetYooStandaloneWinCacheRoot();
#elif UNITY_STANDALONE_LINUX
            return GetYooStandaloneLinuxCacheRoot();
#elif UNITY_STANDALONE_OSX
            return GetYooStandaloneMacCacheRoot();
#else
            return GetYooMobileCacheRoot();
#endif
        }

        /// <summary>
        /// 获取YOO默认的内置文件根目录
        /// </summary>
        internal static string GetYooDefaultBuildinRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return Application.streamingAssetsPath;
            else
                return PathUtility.Combine(Application.streamingAssetsPath, Setting.DefaultYooFolderName);
        }

        internal static string GetRequestYooDefaultBuildinRoot()
        {
            if (string.IsNullOrEmpty(Setting.DefaultYooFolderName))
                return GetRequestStreamingAssetsPath();
            else
                return PathUtility.Combine(GetRequestStreamingAssetsPath(), Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 获取UnityWebRequest StreamingAssets的路径 (OSX and iOS 需要加 file://)
        /// </summary>
        internal static string GetRequestStreamingAssetsPath()
        {
#if UNITY_STANDALONE_OSX || UNITY_IOS
            return $"file://{Application.streamingAssetsPath}";
#else
            return Application.streamingAssetsPath;
#endif
        }
        #endregion
    }
}