using System;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包构建器的持久化设置
    /// </summary>
    public static class BundleBuilderSetting
    {
        /// <summary>
        /// 获取包裹的构建管线名称
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <returns>构建管线名称</returns>
        public static string GetPackageBuildPipeline(string packageName)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_BuildPipelineName";
            string defaultValue = EBuildPipeline.ScriptableBuildPipeline.ToString();
            return EditorPrefs.GetString(key, defaultValue);
        }

        /// <summary>
        /// 设置包裹的构建管线名称
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        public static void SetPackageBuildPipeline(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_BuildPipelineName";
            EditorPrefs.SetString(key, buildPipeline);
        }

        /// <summary>
        /// 获取包裹的压缩选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>压缩选项</returns>
        public static ECompressOption GetPackageCompressOption(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_{nameof(ECompressOption)}";
            return (ECompressOption)EditorPrefs.GetInt(key, (int)ECompressOption.LZ4);
        }

        /// <summary>
        /// 设置包裹的压缩选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="compressOption">压缩选项</param>
        public static void SetPackageCompressOption(string packageName, string buildPipeline, ECompressOption compressOption)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_{nameof(ECompressOption)}";
            EditorPrefs.SetInt(key, (int)compressOption);
        }

        /// <summary>
        /// 获取包裹的文件名样式
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>文件名样式</returns>
        public static EFileNameStyle GetPackageFileNameStyle(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_{nameof(EFileNameStyle)}";
            return (EFileNameStyle)EditorPrefs.GetInt(key, (int)EFileNameStyle.HashName);
        }

        /// <summary>
        /// 设置包裹的文件名样式
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="fileNameStyle">文件名样式</param>
        public static void SetPackageFileNameStyle(string packageName, string buildPipeline, EFileNameStyle fileNameStyle)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_{nameof(EFileNameStyle)}";
            EditorPrefs.SetInt(key, (int)fileNameStyle);
        }

        /// <summary>
        /// 获取包裹的首包资源的拷贝选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>首包资源的拷贝选项</returns>
        public static EBundledCopyOption GetPackageBundledCopyOption(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_{nameof(EBundledCopyOption)}";
            return (EBundledCopyOption)EditorPrefs.GetInt(key, (int)EBundledCopyOption.None);
        }

        /// <summary>
        /// 设置包裹的首包资源的拷贝选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="bundledCopyOption">首包资源的拷贝选项</param>
        public static void SetPackageBundledCopyOption(string packageName, string buildPipeline, EBundledCopyOption bundledCopyOption)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_{nameof(EBundledCopyOption)}";
            EditorPrefs.SetInt(key, (int)bundledCopyOption);
        }

        /// <summary>
        /// 获取包裹的首包资源的拷贝参数
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>首包资源的拷贝参数字符串</returns>
        public static string GetPackageBundledCopyParams(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_BundledCopyParams";
            return EditorPrefs.GetString(key, string.Empty);
        }

        /// <summary>
        /// 设置包裹的首包资源的拷贝参数
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="bundledCopyParams">首包资源的拷贝参数字符串</param>
        public static void SetPackageBundledCopyParams(string packageName, string buildPipeline, string bundledCopyParams)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_BundledCopyParams";
            EditorPrefs.SetString(key, bundledCopyParams);
        }

        /// <summary>
        /// 获取包裹的资源包加密器类名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>资源包加密器的完整类名</returns>
        public static string GetPackageBundleEncryptorClassName(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_BundleEncryptorClassName";
            return EditorPrefs.GetString(key, $"{typeof(EncryptionNone).FullName}");
        }

        /// <summary>
        /// 设置包裹的资源包加密器类名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="encryptorClassName">资源包加密器的完整类名</param>
        public static void SetPackageBundleEncryptorClassName(string packageName, string buildPipeline, string encryptorClassName)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_BundleEncryptorClassName";
            EditorPrefs.SetString(key, encryptorClassName);
        }

        /// <summary>
        /// 获取包裹的资源清单加密器类名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>资源清单加密器的完整类名</returns>
        public static string GetPackageManifestEncryptorClassName(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_ManifestEncryptorClassName";
            return EditorPrefs.GetString(key, $"{typeof(ManifestEncryptorNone).FullName}");
        }

        /// <summary>
        /// 设置包裹的资源清单加密器类名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="encryptionClassName">资源清单加密器的完整类名</param>
        public static void SetPackageManifestEncryptorClassName(string packageName, string buildPipeline, string encryptionClassName)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_ManifestEncryptorClassName";
            EditorPrefs.SetString(key, encryptionClassName);
        }

        /// <summary>
        /// 获取包裹的资源清单解密器类名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>资源清单解密器的完整类名</returns>
        public static string GetPackageManifestDecryptorClassName(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_ManifestDecryptorClassName";
            return EditorPrefs.GetString(key, $"{typeof(ManifestDecryptorNone).FullName}");
        }

        /// <summary>
        /// 设置包裹的资源清单解密器类名
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="encryptionClassName">资源清单解密器的完整类名</param>
        public static void SetPackageManifestDecryptorClassName(string packageName, string buildPipeline, string encryptionClassName)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_ManifestDecryptorClassName";
            EditorPrefs.SetString(key, encryptionClassName);
        }

        /// <summary>
        /// 获取包裹的清空构建缓存选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>是否清空构建缓存</returns>
        public static bool GetPackageClearBuildCache(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_ClearBuildCache";
            return EditorPrefs.GetInt(key, 0) > 0;
        }

        /// <summary>
        /// 设置包裹的清空构建缓存选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="clearBuildCache">是否清空构建缓存</param>
        public static void SetPackageClearBuildCache(string packageName, string buildPipeline, bool clearBuildCache)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_ClearBuildCache";
            EditorPrefs.SetInt(key, clearBuildCache ? 1 : 0);
        }

        /// <summary>
        /// 获取包裹的资源依赖缓存数据库选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <returns>是否使用资源依赖缓存数据库</returns>
        public static bool GetPackageUseAssetDependencyDB(string packageName, string buildPipeline)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_UseAssetDependencyDB";
            return EditorPrefs.GetInt(key, 0) > 0;
        }

        /// <summary>
        /// 设置包裹的资源依赖缓存数据库选项
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="buildPipeline">构建管线名称</param>
        /// <param name="useAssetDependencyDB">是否使用资源依赖缓存数据库</param>
        public static void SetPackageUseAssetDependencyDB(string packageName, string buildPipeline, bool useAssetDependencyDB)
        {
            string key = $"{PlayerSettings.productGUID}_{packageName}_{buildPipeline}_UseAssetDependencyDB";
            EditorPrefs.SetInt(key, useAssetDependencyDB ? 1 : 0);
        }
    }
}
