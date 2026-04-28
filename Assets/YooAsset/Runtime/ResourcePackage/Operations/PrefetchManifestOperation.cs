using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 预取清单操作，用于提前加载指定版本的资源清单。
    /// </summary>
    public sealed class PrefetchManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckParams,
            CheckActiveManifest,
            LoadPackageManifest,
            Done,
        }

        private readonly FileSystemHost _host;
        private readonly PrefetchManifestOptions _options;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private PackageManifest _manifest;
        private ESteps _steps = ESteps.None;


        internal PrefetchManifestOperation(FileSystemHost host, PrefetchManifestOptions options)
        {
            _host = host;
            _options = options;
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            _steps = ESteps.CheckParams;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParams)
            {
                if (string.IsNullOrEmpty(_options.PackageVersion))
                {
                    _steps = ESteps.Done;
                    SetError("Package version is null or empty.");
                    return;
                }

                _steps = ESteps.CheckActiveManifest;
            }

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象
                if (_host.ActiveManifest != null)
                {
                    if (_host.ActiveManifest.PackageVersion == _options.PackageVersion)
                    {
                        _manifest = _host.ActiveManifest;
                        _steps = ESteps.Done;
                        SetResult();
                        return;
                    }
                }
                _steps = ESteps.LoadPackageManifest;
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadPackageManifestOp == null)
                {
                    var mainFileSystem = _host.GetMainFileSystem();
                    if (mainFileSystem == null)
                    {
                        _steps = ESteps.Done;
                        SetError("Main file system is null.");
                        return;
                    }

                    var options = new LoadPackageManifestOptions(_options.PackageVersion, _options.Timeout);
                    _loadPackageManifestOp = mainFileSystem.LoadPackageManifestAsync(options.ConvertTo());
                    _loadPackageManifestOp.StartOperation();
                    AddChildOperation(_loadPackageManifestOp);
                }

                _loadPackageManifestOp.UpdateOperation();
                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _manifest = _loadPackageManifestOp.Manifest;
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadPackageManifestOp.Error);
                }
            }
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件。
        /// </summary>
        /// <param name="options">资源下载选项</param>
        /// <returns>资源下载操作实例</returns>
        public ResourceDownloaderOperation CreateResourceDownloader(ResourceDownloaderOptions options)
        {
            if (Status != EOperationStatus.Succeeded)
            {
                YooLogger.LogError($"{nameof(PrefetchManifestOperation)} did not succeed.");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_host.PackageName);
            }

            return _host.CreateResourceDownloader(_manifest, options);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源信息列表依赖的资源包文件。
        /// </summary>
        /// <param name="options">资源包下载选项</param>
        /// <returns>资源下载操作实例</returns>
        public ResourceDownloaderOperation CreateBundleDownloader(BundleDownloaderOptions options)
        {
            if (Status != EOperationStatus.Succeeded)
            {
                YooLogger.LogError($"{nameof(PrefetchManifestOperation)} did not succeed.");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_host.PackageName);
            }

            return _host.CreateResourceDownloader(_manifest, options);
        }
    }
}