using System;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public static class AssetBundleBuilderSetting
    {
        // BuildPipelineName
        public static string GetPackageBuildPipeline(string packageName)
        {
            string key = $"{Application.productName}_{packageName}_BuildPipelineName";
            string defaultValue = EBuildPipeline.ScriptableBuildPipeline.ToString();
            return EditorPrefs.GetString(key, defaultValue);
        }
        public static void SetPackageBuildPipeline(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_BuildPipelineName";
            EditorPrefs.SetString(key, buildPipeline);
        }

        // ECompressOption
        public static ECompressOption GetPackageCompressOption(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(ECompressOption)}";
            return (ECompressOption)EditorPrefs.GetInt(key, (int)ECompressOption.LZ4);
        }
        public static void SetPackageCompressOption(string packageName, string buildPipeline, ECompressOption compressOption)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(ECompressOption)}";
            EditorPrefs.SetInt(key, (int)compressOption);
        }

        // EFileNameStyle
        public static EFileNameStyle GetPackageFileNameStyle(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EFileNameStyle)}";
            return (EFileNameStyle)EditorPrefs.GetInt(key, (int)EFileNameStyle.HashName);
        }
        public static void SetPackageFileNameStyle(string packageName, string buildPipeline, EFileNameStyle fileNameStyle)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EFileNameStyle)}";
            EditorPrefs.SetInt(key, (int)fileNameStyle);
        }

        // EBuildinFileCopyOption
        public static EBuildinFileCopyOption GetPackageBuildinFileCopyOption(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EBuildinFileCopyOption)}";
            return (EBuildinFileCopyOption)EditorPrefs.GetInt(key, (int)EBuildinFileCopyOption.None);
        }
        public static void SetPackageBuildinFileCopyOption(string packageName, string buildPipeline, EBuildinFileCopyOption buildinFileCopyOption)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EBuildinFileCopyOption)}";
            EditorPrefs.SetInt(key, (int)buildinFileCopyOption);
        }

        // BuildFileCopyParams
        public static string GetPackageBuildinFileCopyParams(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_BuildFileCopyParams";
            return EditorPrefs.GetString(key, string.Empty);
        }
        public static void SetPackageBuildinFileCopyParams(string packageName, string buildPipeline, string buildinFileCopyParams)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_BuildFileCopyParams";
            EditorPrefs.SetString(key, buildinFileCopyParams);
        }

        // EncyptionServicesClassName
        public static string GetPackageEncyptionServicesClassName(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_EncyptionServicesClassName";
            return EditorPrefs.GetString(key, $"{typeof(EncryptionNone).FullName}");
        }
        public static void SetPackageEncyptionServicesClassName(string packageName, string buildPipeline, string encyptionClassName)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_EncyptionServicesClassName";
            EditorPrefs.SetString(key, encyptionClassName);
        }

        // ManifestProcessServicesClassName
        public static string GetPackageManifestProcessServicesClassName(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ManifestProcessServicesClassName";
            return EditorPrefs.GetString(key, $"{typeof(ManifestProcessNone).FullName}");
        }
        public static void SetPackageManifestProcessServicesClassName(string packageName, string buildPipeline, string encyptionClassName)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ManifestProcessServicesClassName";
            EditorPrefs.SetString(key, encyptionClassName);
        }

        // ManifestRestoreServicesClassName
        public static string GetPackageManifestRestoreServicesClassName(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ManifestRestoreServicesClassName";
            return EditorPrefs.GetString(key, $"{typeof(ManifestRestoreNone).FullName}");
        }
        public static void SetPackageManifestRestoreServicesClassName(string packageName, string buildPipeline, string encyptionClassName)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ManifestRestoreServicesClassName";
            EditorPrefs.SetString(key, encyptionClassName);
        }

        // ClearBuildCache
        public static bool GetPackageClearBuildCache(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ClearBuildCache";
            return EditorPrefs.GetInt(key, 0) > 0;
        }
        public static void SetPackageClearBuildCache(string packageName, string buildPipeline, bool clearBuildCache)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ClearBuildCache";
            EditorPrefs.SetInt(key, clearBuildCache ? 1 : 0);
        }

        // UseAssetDependencyDB
        public static bool GetPackageUseAssetDependencyDB(string packageName, string buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_UseAssetDependencyDB";
            return EditorPrefs.GetInt(key, 0) > 0;
        }
        public static void SetPackageUseAssetDependencyDB(string packageName, string buildPipeline, bool useAssetDependencyDB)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_UseAssetDependencyDB";
            EditorPrefs.SetInt(key, useAssetDependencyDB ? 1 : 0);
        }
    }
}