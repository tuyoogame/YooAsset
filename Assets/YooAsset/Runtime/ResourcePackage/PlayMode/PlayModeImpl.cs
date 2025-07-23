using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class PlayModeImpl : IPlayMode, IBundleQuery
    {
        public readonly string PackageName;
        public readonly EPlayMode PlayMode;
        public readonly List<IFileSystem> FileSystems = new List<IFileSystem>(10);

        public PlayModeImpl(string packageName, EPlayMode playMode)
        {
            PackageName = packageName;
            PlayMode = playMode;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(FileSystemParameters fileSystemParameter)
        {
            var fileSystemParamList = new List<FileSystemParameters>();
            if (fileSystemParameter != null)
                fileSystemParamList.Add(fileSystemParameter);
            return InitializeAsync(fileSystemParamList);
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(FileSystemParameters fileSystemParameterA, FileSystemParameters fileSystemParameterB)
        {
            var fileSystemParamList = new List<FileSystemParameters>();
            if (fileSystemParameterA != null)
                fileSystemParamList.Add(fileSystemParameterA);
            if (fileSystemParameterB != null)
                fileSystemParamList.Add(fileSystemParameterB);
            return InitializeAsync(fileSystemParamList);
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(List<FileSystemParameters> fileSystemParameterList)
        {
            var operation = new InitializationOperation(this, fileSystemParameterList);
            return operation;
        }

        #region IPlayMode接口
        /// <summary>
        /// 当前激活的清单
        /// </summary>
        public PackageManifest ActiveManifest { set; get; }

        /// <summary>
        /// 销毁文件系统
        /// </summary>
        void IPlayMode.DestroyFileSystem()
        {
            foreach (var fileSystem in FileSystems)
            {
                fileSystem.OnDestroy();
            }
            FileSystems.Clear();
        }

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        RequestPackageVersionOperation IPlayMode.RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new RequestPackageVersionImplOperation(this, appendTimeTicks, timeout);
            return operation;
        }

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new UpdatePackageManifestOperation(this, packageVersion, timeout);
            return operation;
        }

        /// <summary>
        /// 预下载指定版本的包裹内容
        /// </summary>
        PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
        {
            var operation = new PreDownloadContentOperation(this, packageVersion, timeout);
            return operation;
        }

        /// <summary>
        /// 清理缓存文件
        /// </summary>
        ClearCacheFilesOperation IPlayMode.ClearCacheFilesAsync(ClearCacheFilesOptions options)
        {
            var operation = new ClearCacheFilesOperation(this, options);
            return operation;
        }

        // 下载相关
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> downloadList = GetDownloadListByAll(ActiveManifest);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> downloadList = GetDownloadListByTags(ActiveManifest, tags);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> downloadList = GetDownloadListByPaths(ActiveManifest, assetInfos, recursiveDownload);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }

        // 解压相关
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> unpcakList = GetUnpackListByAll(ActiveManifest);
            var operation = new ResourceUnpackerOperation(PackageName, unpcakList, upackingMaxNumber, failedTryAgain);
            return operation;
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> unpcakList = GetUnpackListByTags(ActiveManifest, tags);
            var operation = new ResourceUnpackerOperation(PackageName, unpcakList, upackingMaxNumber, failedTryAgain);
            return operation;
        }

        // 导入相关
        ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> importerList = GetImporterListByFilePaths(ActiveManifest, filePaths);
            var operation = new ResourceImporterOperation(PackageName, importerList, importingMaxNumber, failedTryAgain);
            return operation;
        }
        ResourceImporterOperation IPlayMode.CreateResourceImporterByFileInfos(ImportFileInfo[] fileInfos, int importingMaxNumber, int failedTryAgain)
        {
            List<BundleInfo> importerList = GetImporterListByFileInfos(ActiveManifest, fileInfos);
            var operation = new ResourceImporterOperation(PackageName, importerList, importingMaxNumber, failedTryAgain);
            return operation;
        }
        #endregion

        #region IBundleQuery接口
        private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
        {
            if (packageBundle == null)
                throw new Exception("Should never get here !");

            var fileSystem = GetBelongFileSystem(packageBundle);
            if (fileSystem != null)
            {
                BundleInfo bundleInfo = new BundleInfo(fileSystem, packageBundle);
                return bundleInfo;
            }

            throw new Exception($"Can not found belong file system : {packageBundle.BundleName}");
        }
        BundleInfo IBundleQuery.GetMainBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo == null || assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = ActiveManifest.GetMainPackageBundle(assetInfo.Asset);
            return CreateBundleInfo(packageBundle);
        }
        List<BundleInfo> IBundleQuery.GetDependBundleInfos(AssetInfo assetInfo)
        {
            if (assetInfo == null || assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            List<PackageBundle> depends;
            if (assetInfo.LoadMethod == AssetInfo.ELoadMethod.LoadAllAssets)
            {
                var mainBundle = ActiveManifest.GetMainPackageBundle(assetInfo.Asset);
                depends = ActiveManifest.GetBundleAllDependencies(mainBundle);
            }
            else
            {
                depends = ActiveManifest.GetAssetAllDependencies(assetInfo.Asset);
            }

            List<BundleInfo> result = new List<BundleInfo>(depends.Count);
            foreach (var packageBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(packageBundle);
                result.Add(bundleInfo);
            }
            return result;
        }
        string IBundleQuery.GetMainBundleName(int bundleID)
        {
            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = ActiveManifest.GetMainPackageBundle(bundleID);
            return packageBundle.BundleName;
        }
        #endregion

        /// <summary>
        /// 获取主文件系统
        ///  说明：文件系统列表里，最后一个属于主文件系统
        /// </summary>
        public IFileSystem GetMainFileSystem()
        {
            int count = FileSystems.Count;
            if (count == 0)
                return null;
            return FileSystems[count - 1];
        }

        /// <summary>
        /// 获取资源包所属文件系统
        /// </summary>
        public IFileSystem GetBelongFileSystem(PackageBundle packageBundle)
        {
            for (int i = 0; i < FileSystems.Count; i++)
            {
                IFileSystem fileSystem = FileSystems[i];
                if (fileSystem.Belong(packageBundle))
                {
                    return fileSystem;
                }
            }

            YooLogger.Error($"Can not found belong file system : {packageBundle.BundleName}");
            return null;
        }

        public List<BundleInfo> GetDownloadListByAll(PackageManifest manifest)
        {
            if (manifest == null)
                return new List<BundleInfo>();

            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                var fileSystem = GetBelongFileSystem(packageBundle);
                if (fileSystem == null)
                    continue;

                if (fileSystem.NeedDownload(packageBundle))
                {
                    var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                    result.Add(bundleInfo);
                }
            }
            return result;
        }
        public List<BundleInfo> GetDownloadListByTags(PackageManifest manifest, string[] tags)
        {
            if (manifest == null)
                return new List<BundleInfo>();

            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                var fileSystem = GetBelongFileSystem(packageBundle);
                if (fileSystem == null)
                    continue;

                if (fileSystem.NeedDownload(packageBundle))
                {
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
            }
            return result;
        }
        public List<BundleInfo> GetDownloadListByPaths(PackageManifest manifest, AssetInfo[] assetInfos, bool recursiveDownload)
        {
            if (manifest == null)
                return new List<BundleInfo>();

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
                PackageBundle mainBundle = manifest.GetMainPackageBundle(assetInfo.Asset);
                if (checkList.Contains(mainBundle) == false)
                    checkList.Add(mainBundle);

                // 注意：如果清单里未找到资源包会抛出异常！
                List<PackageBundle> mainDependBundles = manifest.GetAssetAllDependencies(assetInfo.Asset);
                foreach (var dependBundle in mainDependBundles)
                {
                    if (checkList.Contains(dependBundle) == false)
                        checkList.Add(dependBundle);
                }

                // 下载主资源包内所有资源对象依赖的资源包
                if (recursiveDownload)
                {
                    foreach (var otherMainAsset in mainBundle.IncludeMainAssets)
                    {
                        var otherMainBundle = manifest.GetMainPackageBundle(otherMainAsset.BundleID);
                        if (checkList.Contains(otherMainBundle) == false)
                            checkList.Add(otherMainBundle);

                        List<PackageBundle> otherDependBundles = manifest.GetAssetAllDependencies(otherMainAsset);
                        foreach (var dependBundle in otherDependBundles)
                        {
                            if (checkList.Contains(dependBundle) == false)
                                checkList.Add(dependBundle);
                        }
                    }
                }
            }

            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in checkList)
            {
                var fileSystem = GetBelongFileSystem(packageBundle);
                if (fileSystem == null)
                    continue;

                if (fileSystem.NeedDownload(packageBundle))
                {
                    var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                    result.Add(bundleInfo);
                }
            }
            return result;
        }
        public List<BundleInfo> GetUnpackListByAll(PackageManifest manifest)
        {
            if (manifest == null)
                return new List<BundleInfo>();

            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                var fileSystem = GetBelongFileSystem(packageBundle);
                if (fileSystem == null)
                    continue;

                if (fileSystem.NeedUnpack(packageBundle))
                {
                    var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                    result.Add(bundleInfo);
                }
            }
            return result;
        }
        public List<BundleInfo> GetUnpackListByTags(PackageManifest manifest, string[] tags)
        {
            if (manifest == null)
                return new List<BundleInfo>();

            List<BundleInfo> result = new List<BundleInfo>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                var fileSystem = GetBelongFileSystem(packageBundle);
                if (fileSystem == null)
                    continue;

                if (fileSystem.NeedUnpack(packageBundle))
                {
                    if (packageBundle.HasTag(tags))
                    {
                        var bundleInfo = new BundleInfo(fileSystem, packageBundle);
                        result.Add(bundleInfo);
                    }
                }
            }
            return result;
        }
        public List<BundleInfo> GetImporterListByFilePaths(PackageManifest manifest, string[] filePaths)
        {
            if (manifest == null)
                return new List<BundleInfo>();

            ImportFileInfo[] fileInfos = new ImportFileInfo[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                ImportFileInfo fileInfo = new ImportFileInfo();
                fileInfo.FilePath = filePaths[i];
                fileInfos[i] = fileInfo;
            }

            return GetImporterListByFileInfos(manifest, fileInfos);
        }
        public List<BundleInfo> GetImporterListByFileInfos(PackageManifest manifest, ImportFileInfo[] fileInfos)
        {
            if (manifest == null)
                return new List<BundleInfo>();

            List<BundleInfo> result = new List<BundleInfo>();
            foreach (var fileInfo in fileInfos)
            {
                string filePath = fileInfo.FilePath;
                if (string.IsNullOrEmpty(filePath))
                    continue;

                PackageBundle packageBundle = null;
                if (string.IsNullOrEmpty(fileInfo.BundleName) == false)
                {
                    if (manifest.TryGetPackageBundleByBundleName(fileInfo.BundleName, out packageBundle) == false)
                    {
                        YooLogger.Warning($"Not found package bundle, bundle name : {fileInfo.BundleName}");
                        continue;
                    }
                }
                else if (string.IsNullOrEmpty(fileInfo.BundleGUID) == false)
                {
                    if (manifest.TryGetPackageBundleByBundleGUID(fileInfo.BundleGUID, out packageBundle) == false)
                    {
                        YooLogger.Warning($"Not found package bundle, bundle guid : {fileInfo.BundleGUID}");
                        continue;
                    }
                }
                else
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    if (manifest.TryGetPackageBundleByFileName(fileName, out packageBundle) == false)
                    {
                        YooLogger.Warning($"Not found package bundle, file name : {fileName}");
                        continue;
                    }
                }

                if (packageBundle != null)
                {
                    var fileSystem = GetBelongFileSystem(packageBundle);
                    if (fileSystem == null)
                        continue;

                    if (fileSystem.NeedImport(packageBundle))
                    {
                        var bundleInfo = new BundleInfo(fileSystem, packageBundle, filePath);
                        result.Add(bundleInfo);
                    }
                }
            }
            return result;
        }
    }
}