using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class HostPlayModeImpl : IPlayMode, IBundleQuery
    {
        public readonly string PackageName;
        public IFileSystem BuildinFileSystem { set; get; } //可以为空！
        public IFileSystem DeliveryFileSystem { set; get; } //可以为空！
        public IFileSystem CacheFileSystem { set; get; }


        public HostPlayModeImpl(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(HostPlayModeParameters initParameters)
        {
            var operation = new HostPlayModeInitializationOperation(this, initParameters);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        #region IPlayMode接口
        public PackageManifest ActiveManifest { set; get; }

        void IPlayMode.UpdatePlayMode()
        {
            if (BuildinFileSystem != null)
                BuildinFileSystem.OnUpdate();

            if (DeliveryFileSystem != null)
                DeliveryFileSystem.OnUpdate();

            if (CacheFileSystem != null)
                CacheFileSystem.OnUpdate();
        }

        RequestPackageVersionOperation IPlayMode.RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new RequestPackageVersionImplOperation(CacheFileSystem, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new UpdatePackageManifestImplOperation(this, CacheFileSystem, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
        {
            var operation = new HostPlayModePreDownloadContentOperation(this, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        ClearAllBundleFilesOperation IPlayMode.ClearAllBundleFilesAsync()
        {
            var operation = new ClearAllBundleFilesImplOperation(this, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        ClearUnusedBundleFilesOperation IPlayMode.ClearUnusedBundleFilesAsync()
        {
            var operation = new ClearUnusedBundleFilesImplOperation(this, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByAll(ActiveManifest, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByTags(ActiveManifest, tags, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByPaths(ActiveManifest, assetInfos, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            var operation = new ResourceDownloaderOperation(PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = PlayModeHelper.GetUnpackListByAll(ActiveManifest, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            var operation = new ResourceUnpackerOperation(PackageName, unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = PlayModeHelper.GetUnpackListByTags(ActiveManifest, tags, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            var operation = new ResourceUnpackerOperation(PackageName, unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> importerList = PlayModeHelper.GetImporterListByFilePaths(ActiveManifest, filePaths, BuildinFileSystem, DeliveryFileSystem, CacheFileSystem);
            var operation = new ResourceImporterOperation(PackageName, importerList, importerMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        #endregion

        #region IBundleQuery接口
        private BundleInfo CreateBundleInfo(PackageBundle packageBundle, AssetInfo assetInfo)
        {
            if (packageBundle == null)
                throw new Exception("Should never get here !");

            if (BuildinFileSystem != null && BuildinFileSystem.Belong(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(BuildinFileSystem, packageBundle);
                return bundleInfo;
            }
            if (DeliveryFileSystem != null && DeliveryFileSystem.Belong(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(DeliveryFileSystem, packageBundle);
                return bundleInfo;
            }
            if (CacheFileSystem.Belong(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(CacheFileSystem, packageBundle);
                return bundleInfo;
            }

            throw new Exception($"Can not found belong file system : {packageBundle.BundleName}");
        }
        BundleInfo IBundleQuery.GetMainBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = ActiveManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return CreateBundleInfo(packageBundle, assetInfo);
        }
        BundleInfo[] IBundleQuery.GetDependBundleInfos(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = ActiveManifest.GetAllDependencies(assetInfo.AssetPath);
            List<BundleInfo> result = new List<BundleInfo>(depends.Length);
            foreach (var packageBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(packageBundle, assetInfo);
                result.Add(bundleInfo);
            }
            return result.ToArray();
        }
        string IBundleQuery.GetMainBundleName(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = ActiveManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return packageBundle.BundleName;
        }
        string[] IBundleQuery.GetDependBundleNames(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = ActiveManifest.GetAllDependencies(assetInfo.AssetPath);
            List<string> result = new List<string>(depends.Length);
            foreach (var packageBundle in depends)
            {
                result.Add(packageBundle.BundleName);
            }
            return result.ToArray();
        }
        bool IBundleQuery.ManifestValid()
        {
            return ActiveManifest != null;
        }
        #endregion
    }
}