using System;
using System.Collections.Generic;

namespace YooAsset
{
    internal class PlayModeHelper
    {
        public static IFileSystem CreateFileSystem(string packageName, FileSystemParameters parameters)
        {
            Type classType = Type.GetType(parameters.FileSystemClass);
            if (classType == null)
            {
                YooLogger.Error($"Can not found file system class type {parameters.FileSystemClass}");
                return null;
            }

            var instance = (IFileSystem)System.Activator.CreateInstance(classType, true);
            if (instance == null)
            {
                YooLogger.Error($"Failed to create file system instance {parameters.FileSystemClass}");
                return null;
            }

            foreach (var param in parameters.CreateParameters)
            {
                instance.SetParameter(param.Key, param.Value);
            }
            instance.OnCreate(packageName, parameters.RootDirectory);
            return instance;
        }

        public static List<BundleInfo> GetDownloadListByAll(PackageManifest manifest, IFileSystem fileSystemA = null, IFileSystem fileSystemB = null, IFileSystem fileSystemC = null)
        {
            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                IFileSystem fileSystem = null;
                if (fileSystemA != null && fileSystemA.Belong(packageBundle))
                {
                    if (fileSystemA.NeedDownload(packageBundle))
                        fileSystem = fileSystemA;
                }
                else if (fileSystemB != null && fileSystemB.Belong(packageBundle))
                {
                    if (fileSystemB.NeedDownload(packageBundle))
                        fileSystem = fileSystemB;
                }
                else if (fileSystemC != null && fileSystemC.Belong(packageBundle))
                {
                    if (fileSystemC.NeedDownload(packageBundle))
                        fileSystem = fileSystemC;
                }
                else
                {
                    YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
                }
                if (fileSystem == null)
                    continue;

                var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                result.Add(bundleInfo);
            }
            return result;
        }
        public static List<BundleInfo> GetDownloadListByTags(PackageManifest manifest, string[] tags, IFileSystem fileSystemA = null, IFileSystem fileSystemB = null, IFileSystem fileSystemC = null)
        {
            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                IFileSystem fileSystem = null;
                if (fileSystemA != null && fileSystemA.Belong(packageBundle))
                {
                    if (fileSystemA.NeedDownload(packageBundle))
                        fileSystem = fileSystemA;
                }
                else if (fileSystemB != null && fileSystemB.Belong(packageBundle))
                {
                    if (fileSystemB.NeedDownload(packageBundle))
                        fileSystem = fileSystemB;
                }
                else if (fileSystemC != null && fileSystemC.Belong(packageBundle))
                {
                    if (fileSystemC.NeedDownload(packageBundle))
                        fileSystem = fileSystemC;
                }
                else
                {
                    YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
                }
                if (fileSystem == null)
                    continue;

                // 如果未带任何标记，则统一下载
                if (packageBundle.HasAnyTags() == false)
                {
                    var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                    result.Add(bundleInfo);
                }
                else
                {
                    // 查询DLC资源
                    if (packageBundle.HasTag(tags))
                    {
                        var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                        result.Add(bundleInfo);
                    }
                }
            }
            return result;
        }
        public static List<BundleInfo> GetDownloadListByPaths(PackageManifest manifest, AssetInfo[] assetInfos, IFileSystem fileSystemA = null, IFileSystem fileSystemB = null, IFileSystem fileSystemC = null)
        {
            // 获取资源对象的资源包和所有依赖资源包
            List<PackageBundle> checkList = new List<PackageBundle>();
            foreach (var assetInfo in assetInfos)
            {
                if (assetInfo.IsInvalid)
                {
                    YooLogger.Warning(assetInfo.Error);
                    continue;
                }

                // 注意：如果清单里未找到资源包会抛出异常！
                PackageBundle mainBundle = manifest.GetMainPackageBundle(assetInfo.AssetPath);
                if (checkList.Contains(mainBundle) == false)
                    checkList.Add(mainBundle);

                // 注意：如果清单里未找到资源包会抛出异常！
                PackageBundle[] dependBundles = manifest.GetAllDependencies(assetInfo.AssetPath);
                foreach (var dependBundle in dependBundles)
                {
                    if (checkList.Contains(dependBundle) == false)
                        checkList.Add(dependBundle);
                }
            }

            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in checkList)
            {
                IFileSystem fileSystem = null;
                if (fileSystemA != null && fileSystemA.Belong(packageBundle))
                {
                    if (fileSystemA.NeedDownload(packageBundle))
                        fileSystem = fileSystemA;
                }
                else if (fileSystemB != null && fileSystemB.Belong(packageBundle))
                {
                    if (fileSystemB.NeedDownload(packageBundle))
                        fileSystem = fileSystemB;
                }
                else if (fileSystemC != null && fileSystemC.Belong(packageBundle))
                {
                    if (fileSystemC.NeedDownload(packageBundle))
                        fileSystem = fileSystemC;
                }
                else
                {
                    YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
                }
                if (fileSystem == null)
                    continue;

                var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                result.Add(bundleInfo);
            }
            return result;
        }
        public static List<BundleInfo> GetUnpackListByAll(PackageManifest manifest, IFileSystem fileSystemA = null, IFileSystem fileSystemB = null, IFileSystem fileSystemC = null)
        {
            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                IFileSystem fileSystem = null;
                if (fileSystemA != null && fileSystemA.Belong(packageBundle))
                {
                    if (fileSystemA.NeedUnpack(packageBundle))
                        fileSystem = fileSystemA;
                }
                else if (fileSystemB != null && fileSystemB.Belong(packageBundle))
                {
                    if (fileSystemB.NeedUnpack(packageBundle))
                        fileSystem = fileSystemB;
                }
                else if (fileSystemC != null && fileSystemC.Belong(packageBundle))
                {
                    if (fileSystemC.NeedUnpack(packageBundle))
                        fileSystem = fileSystemC;
                }
                else
                {
                    YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
                }
                if (fileSystem == null)
                    continue;

                var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                result.Add(bundleInfo);
            }

            return result;
        }
        public static List<BundleInfo> GetUnpackListByTags(PackageManifest manifest, string[] tags, IFileSystem fileSystemA = null, IFileSystem fileSystemB = null, IFileSystem fileSystemC = null)
        {
            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                IFileSystem fileSystem = null;
                if (fileSystemA != null && fileSystemA.Belong(packageBundle))
                {
                    if (fileSystemA.NeedUnpack(packageBundle))
                        fileSystem = fileSystemA;
                }
                else if (fileSystemB != null && fileSystemB.Belong(packageBundle))
                {
                    if (fileSystemB.NeedUnpack(packageBundle))
                        fileSystem = fileSystemB;
                }
                else if (fileSystemC != null && fileSystemC.Belong(packageBundle))
                {
                    if (fileSystemC.NeedUnpack(packageBundle))
                        fileSystem = fileSystemC;
                }
                else
                {
                    YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
                }
                if (fileSystem == null)
                    continue;

                // 查询DLC资源
                if (packageBundle.HasTag(tags))
                {
                    var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                    result.Add(bundleInfo);
                }
            }

            return result;
        }
        public static List<BundleInfo> GetImporterListByFilePaths(PackageManifest manifest, string[] filePaths, IFileSystem fileSystemA = null, IFileSystem fileSystemB = null, IFileSystem fileSystemC = null)
        {
            List<BundleInfo> result = new List<BundleInfo>();
            foreach (var filePath in filePaths)
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                if (manifest.TryGetPackageBundleByFileName(fileName, out PackageBundle packageBundle))
                {
                    IFileSystem fileSystem = null;
                    if (fileSystemA != null && fileSystemA.Belong(packageBundle))
                    {
                        if (fileSystemA.NeedImport(packageBundle))
                            fileSystem = fileSystemA;
                    }
                    else if (fileSystemB != null && fileSystemB.Belong(packageBundle))
                    {
                        if (fileSystemB.NeedImport(packageBundle))
                            fileSystem = fileSystemB;
                    }
                    else if (fileSystemC != null && fileSystemC.Belong(packageBundle))
                    {
                        if (fileSystemC.NeedImport(packageBundle))
                            fileSystem = fileSystemC;
                    }
                    else
                    {
                        YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
                    }
                    if (fileSystem == null)
                        continue;

                    var bundleInfo = new BundleInfo(fileSystem, packageBundle, filePath);
                    result.Add(bundleInfo);
                }
                else
                {
                    YooLogger.Warning($"Not found package bundle, importer file path : {filePath}");
                }
            }
            return result;
        }
    }
}