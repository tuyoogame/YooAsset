using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskCreateReport
    {
        protected void CreateReportFile(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext, ManifestContext manifestContext)
        {
            var buildParameters = buildParametersContext.Parameters;

            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            PackageManifest manifest = manifestContext.Manifest;
            BuildReport buildReport = new BuildReport();

            // 概述信息
            {
                buildReport.Summary.YooVersion = EditorTools.GetPackageManagerYooVersion();
                buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
                buildReport.Summary.BuildDate = DateTime.Now.ToString();
                buildReport.Summary.BuildSeconds = BuildRunner.TotalSeconds;
                buildReport.Summary.BuildTarget = buildParameters.BuildTarget;
                buildReport.Summary.BuildPipeline = buildParameters.BuildPipeline;
                buildReport.Summary.BuildBundleType = buildParameters.BuildBundleType;
                buildReport.Summary.BuildPackageName = buildParameters.PackageName;
                buildReport.Summary.BuildPackageVersion = buildParameters.PackageVersion;
                buildReport.Summary.BuildPackageNote = buildParameters.PackageNote;

                // 收集器配置
                buildReport.Summary.UniqueBundleName = buildMapContext.Command.UniqueBundleName;
                buildReport.Summary.EnableAddressable = buildMapContext.Command.EnableAddressable;
                buildReport.Summary.SupportExtensionless = buildMapContext.Command.SupportExtensionless;
                buildReport.Summary.LocationToLower = buildMapContext.Command.LocationToLower;
                buildReport.Summary.IncludeAssetGUID = buildMapContext.Command.IncludeAssetGUID;
                buildReport.Summary.AutoCollectShaders = buildMapContext.Command.AutoCollectShaders;
                buildReport.Summary.IgnoreRuleName = buildMapContext.Command.IgnoreRule.GetType().FullName;

                // 构建参数
                buildReport.Summary.ClearBuildCacheFiles = buildParameters.ClearBuildCacheFiles;
                buildReport.Summary.UseAssetDependencyDB = buildParameters.UseAssetDependencyDB;
                buildReport.Summary.EnableSharePackRule = buildParameters.EnableSharePackRule;
                buildReport.Summary.SingleReferencedPackAlone = buildParameters.SingleReferencedPackAlone;
                buildReport.Summary.FileNameStyle = buildParameters.FileNameStyle;
                buildReport.Summary.EncryptionServicesClassName = buildParameters.EncryptionServices == null ? "null" : buildParameters.EncryptionServices.GetType().FullName;
                buildReport.Summary.ManifestProcessServicesClassName = buildParameters.ManifestProcessServices == null ? "null" : buildParameters.ManifestProcessServices.GetType().FullName;
                buildReport.Summary.ManifestRestoreServicesClassName = buildParameters.ManifestRestoreServices == null ? "null" : buildParameters.ManifestRestoreServices.GetType().FullName;

                if (buildParameters is BuiltinBuildParameters)
                {
                    var builtinBuildParameters = buildParameters as BuiltinBuildParameters;
                    buildReport.Summary.CompressOption = builtinBuildParameters.CompressOption;
                    buildReport.Summary.DisableWriteTypeTree = builtinBuildParameters.DisableWriteTypeTree;
                    buildReport.Summary.IgnoreTypeTreeChanges = builtinBuildParameters.IgnoreTypeTreeChanges;
                    buildReport.Summary.ReplaceAssetPathWithAddress = builtinBuildParameters.ReplaceAssetPathWithAddress;
                }
                else if (buildParameters is ScriptableBuildParameters)
                {
                    var scriptableBuildParameters = buildParameters as ScriptableBuildParameters;
                    buildReport.Summary.CompressOption = scriptableBuildParameters.CompressOption;
                    buildReport.Summary.DisableWriteTypeTree = scriptableBuildParameters.DisableWriteTypeTree;
                    buildReport.Summary.IgnoreTypeTreeChanges = scriptableBuildParameters.IgnoreTypeTreeChanges;
                    buildReport.Summary.ReplaceAssetPathWithAddress = scriptableBuildParameters.ReplaceAssetPathWithAddress;
                    buildReport.Summary.WriteLinkXML = scriptableBuildParameters.WriteLinkXML;
                    buildReport.Summary.CacheServerHost = scriptableBuildParameters.CacheServerHost;
                    buildReport.Summary.CacheServerPort = scriptableBuildParameters.CacheServerPort;
                    buildReport.Summary.BuiltinShadersBundleName = scriptableBuildParameters.BuiltinShadersBundleName;
                    buildReport.Summary.MonoScriptsBundleName = scriptableBuildParameters.MonoScriptsBundleName;
                }

                // 构建结果
                buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
                buildReport.Summary.MainAssetTotalCount = GetMainAssetCount(manifest);
                buildReport.Summary.AllBundleTotalCount = GetAllBundleCount(manifest);
                buildReport.Summary.AllBundleTotalSize = GetAllBundleSize(manifest);
                buildReport.Summary.EncryptedBundleTotalCount = GetEncryptedBundleCount(manifest);
                buildReport.Summary.EncryptedBundleTotalSize = GetEncryptedBundleSize(manifest);
            }

            // 资源对象列表
            buildReport.AssetInfos = new List<ReportAssetInfo>(manifest.AssetList.Count);
            foreach (var packageAsset in manifest.AssetList)
            {
                var mainBundle = manifest.BundleList[packageAsset.BundleID];
                ReportAssetInfo reportAssetInfo = new ReportAssetInfo();
                reportAssetInfo.Address = packageAsset.Address;
                reportAssetInfo.AssetPath = packageAsset.AssetPath;
                reportAssetInfo.AssetTags = packageAsset.AssetTags;
                reportAssetInfo.AssetGUID = AssetDatabase.AssetPathToGUID(packageAsset.AssetPath);
                reportAssetInfo.MainBundleName = mainBundle.BundleName;
                reportAssetInfo.MainBundleSize = mainBundle.FileSize;
                reportAssetInfo.DependAssets = GetAssetDependAssets(buildMapContext, mainBundle.BundleName, packageAsset.AssetPath);
                reportAssetInfo.DependBundles = GetAssetDependBundles(manifest, packageAsset);
                buildReport.AssetInfos.Add(reportAssetInfo);
            }

            // 资源包列表
            buildReport.BundleInfos = new List<ReportBundleInfo>(manifest.BundleList.Count);
            foreach (var packageBundle in manifest.BundleList)
            {
                ReportBundleInfo reportBundleInfo = new ReportBundleInfo();
                reportBundleInfo.BundleName = packageBundle.BundleName;
                reportBundleInfo.FileName = packageBundle.FileName;
                reportBundleInfo.FileHash = packageBundle.FileHash;
                reportBundleInfo.FileCRC = packageBundle.FileCRC;
                reportBundleInfo.FileSize = packageBundle.FileSize;
                reportBundleInfo.Encrypted = packageBundle.Encrypted;
                reportBundleInfo.Tags = packageBundle.Tags;
                reportBundleInfo.DependBundles = GetBundleDependBundles(manifest, packageBundle);
                reportBundleInfo.ReferenceBundles = GetBundleReferenceBundles(manifest, packageBundle);
                reportBundleInfo.BundleContents = GetBundleContents(buildMapContext, packageBundle.BundleName);
                buildReport.BundleInfos.Add(reportBundleInfo);
            }

            // 其它资源列表
            buildReport.IndependAssets = new List<ReportIndependAsset>(buildMapContext.IndependAssets);

            // 序列化文件
            string fileName = YooAssetSettingsData.GetBuildReportFileName(buildParameters.PackageName, buildParameters.PackageVersion);
            string filePath = $"{packageOutputDirectory}/{fileName}";
            BuildReport.Serialize(filePath, buildReport);
            BuildLogger.Log($"Create build report file: {filePath}");
        }

        /// <summary>
        /// 获取资源对象依赖的其它所有资源
        /// </summary>
        private List<AssetInfo> GetAssetDependAssets(BuildMapContext buildMapContext, string bundleName, string assetPath)
        {
            List<AssetInfo> result = new List<AssetInfo>();
            var bundleInfo = buildMapContext.GetBundleInfo(bundleName);
            var assetInfo = bundleInfo.GetPackAssetInfo(assetPath);
            foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
            {
                result.Add(dependAssetInfo.AssetInfo);
            }
            result.Sort();
            return result;
        }

        /// <summary>
        /// 获取资源对象依赖的资源包集合
        /// </summary>
        private List<string> GetAssetDependBundles(PackageManifest manifest, PackageAsset packageAsset)
        {
            List<string> dependBundles = new List<string>(packageAsset.DependBundleIDs.Length);
            foreach (int index in packageAsset.DependBundleIDs)
            {
                string dependBundleName = manifest.BundleList[index].BundleName;
                dependBundles.Add(dependBundleName);
            }
            dependBundles.Sort();
            return dependBundles;
        }

        /// <summary>
        /// 获取资源包依赖的资源包集合
        /// </summary>
        private List<string> GetBundleDependBundles(PackageManifest manifest, PackageBundle packageBundle)
        {
            List<string> dependBundles = new List<string>(packageBundle.DependBundleIDs.Length);
            foreach (int index in packageBundle.DependBundleIDs)
            {
                string dependBundleName = manifest.BundleList[index].BundleName;
                dependBundles.Add(dependBundleName);
            }
            dependBundles.Sort();
            return dependBundles;
        }

        /// <summary>
        /// 获取引用该资源包的资源包集合
        /// </summary>
        private List<string> GetBundleReferenceBundles(PackageManifest manifest, PackageBundle packageBundle)
        {
            List<string> referenceBundles = new List<string>(packageBundle.ReferenceBundleIDs.Count);
            foreach (int index in packageBundle.ReferenceBundleIDs)
            {
                string dependBundleName = manifest.BundleList[index].BundleName;
                referenceBundles.Add(dependBundleName);
            }
            referenceBundles.Sort();
            return referenceBundles;
        }

        /// <summary>
        /// 获取资源包内部所有资产
        /// </summary>
        private List<AssetInfo> GetBundleContents(BuildMapContext buildMapContext, string bundleName)
        {
            var bundleInfo = buildMapContext.GetBundleInfo(bundleName);
            List<AssetInfo> result = bundleInfo.GetBundleContents();
            result.Sort();
            return result;
        }

        private int GetMainAssetCount(PackageManifest manifest)
        {
            return manifest.AssetList.Count;
        }
        private int GetAllBundleCount(PackageManifest manifest)
        {
            return manifest.BundleList.Count;
        }
        private long GetAllBundleSize(PackageManifest manifest)
        {
            long fileBytes = 0;
            foreach (var packageBundle in manifest.BundleList)
            {
                fileBytes += packageBundle.FileSize;
            }
            return fileBytes;
        }
        private int GetEncryptedBundleCount(PackageManifest manifest)
        {
            int fileCount = 0;
            foreach (var packageBundle in manifest.BundleList)
            {
                if (packageBundle.Encrypted)
                    fileCount++;
            }
            return fileCount;
        }
        private long GetEncryptedBundleSize(PackageManifest manifest)
        {
            long fileBytes = 0;
            foreach (var packageBundle in manifest.BundleList)
            {
                if (packageBundle.Encrypted)
                    fileBytes += packageBundle.FileSize;
            }
            return fileBytes;
        }
    }
}