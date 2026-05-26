
namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统的加载包裹清单操作
    /// </summary>
    internal sealed class WNFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestPackageHash,
            LoadPackageManifest,
            Done,
        }

        private readonly WebNetworkFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private RequestWebPackageHashOperation _requestWebPackageHashOp;
        private LoadWebPackageManifestOperation _loadWebPackageManifestOp;
        private ESteps _steps = ESteps.None;

        public WNFSLoadPackageManifestOperation(WebNetworkFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RequestPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_requestWebPackageHashOp == null)
                {
                    var options = new RequestWebPackageHashOptions(
                        packageName: _fileSystem.PackageName,
                        packageVersion: _packageVersion,
                        timeout: _timeout,
                        remoteService: _fileSystem.RemoteService,
                        downloadBackend: _fileSystem.DownloadBackend,
                        downloadUrlPolicy: _fileSystem.DownloadUrlPolicy);
                    _requestWebPackageHashOp = new RequestWebPackageHashOperation(options);
                    _requestWebPackageHashOp.StartOperation();
                    AddChildOperation(_requestWebPackageHashOp);
                }

                _requestWebPackageHashOp.UpdateOperation();
                if (_requestWebPackageHashOp.IsDone == false)
                    return;

                if (_requestWebPackageHashOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_requestWebPackageHashOp.Error);
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadWebPackageManifestOp == null)
                {
                    var options = new LoadWebPackageManifestOptions(
                        packageName: _fileSystem.PackageName,
                        packageVersion: _packageVersion,
                        packageHash: _requestWebPackageHashOp.PackageHash,
                        timeout: _timeout,
                        remoteService: _fileSystem.RemoteService,
                        manifestDecryptor: _fileSystem.ManifestDecryptor,
                        downloadBackend: _fileSystem.DownloadBackend,
                        downloadUrlPolicy: _fileSystem.DownloadUrlPolicy);
                    _loadWebPackageManifestOp = new LoadWebPackageManifestOperation(options);
                    _loadWebPackageManifestOp.StartOperation();
                    AddChildOperation(_loadWebPackageManifestOp);
                }

                _loadWebPackageManifestOp.UpdateOperation();
                Progress = _loadWebPackageManifestOp.Progress;
                if (_loadWebPackageManifestOp.IsDone == false)
                    return;

                if (_loadWebPackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadWebPackageManifestOp.Manifest;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadWebPackageManifestOp.Error);
                }
            }
        }
    }
}
