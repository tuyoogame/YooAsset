using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    public sealed class PreDownloadContentOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckParams,
            CheckActiveManifest,
            LoadPackageManifest,
            Done,
        }

        private readonly PlayModeImpl _impl;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private PackageManifest _manifest;
        private ESteps _steps = ESteps.None;


        internal PreDownloadContentOperation(PlayModeImpl impl, string packageVersion, int timeout)
        {
            _impl = impl;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckParams;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParams)
            {
                if (string.IsNullOrEmpty(_packageVersion))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Package version is null or empty.";
                    return;
                }

                _steps = ESteps.CheckActiveManifest;
            }

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象
                if (_impl.ActiveManifest != null)
                {
                    if (_impl.ActiveManifest.PackageVersion == _packageVersion)
                    {
                        _manifest = _impl.ActiveManifest;
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        return;
                    }
                }
                _steps = ESteps.LoadPackageManifest;
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadPackageManifestOp == null)
                {
                    var mainFileSystem = _impl.GetMainFileSystem();
                    _loadPackageManifestOp = mainFileSystem.LoadPackageManifestAsync(_packageVersion, _timeout);
                    _loadPackageManifestOp.StartOperation();
                    AddChildOperation(_loadPackageManifestOp);
                }

                _loadPackageManifestOp.UpdateOperation();
                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _manifest = _loadPackageManifestOp.Manifest;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadPackageManifestOp.Error;
                }
            }
        }

        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByAll(_manifest);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByTags(_manifest, new string[] { tag });
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByTags(_manifest, tags);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public ResourceDownloaderOperation CreateBundleDownloader(string location, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain);
            }

            List<AssetInfo> assetInfos = new List<AssetInfo>();
            var assetInfo = _manifest.ConvertLocationToAssetInfo(location, null);
            assetInfos.Add(assetInfo);

            List<BundleInfo> downloadList = _impl.GetDownloadListByPaths(_manifest, assetInfos.ToArray(), recursiveDownload);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain);
            }

            List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
            foreach (var location in locations)
            {
                var assetInfo = _manifest.ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByPaths(_manifest, assetInfos.ToArray(), recursiveDownload);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain);
            return operation;
        }
    }
}