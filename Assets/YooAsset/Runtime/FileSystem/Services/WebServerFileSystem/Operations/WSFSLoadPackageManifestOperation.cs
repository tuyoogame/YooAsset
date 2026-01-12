
namespace YooAsset
{
    /// <summary>
    /// Web服务端文件系统的加载包裹清单操作
    /// </summary>
    internal sealed class WSFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestWebPackageHash,
            LoadWebPackageManifest,
            Done,
        }

        private readonly WebServerFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private RequestWebServerPackageHashOperation _requestWebPackageHashOp;
        private LoadWebServerPackageManifestOperation _loadWebPackageManifestOp;
        private ESteps _steps = ESteps.None;


        public WSFSLoadPackageManifestOperation(WebServerFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RequestWebPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestWebPackageHash)
            {
                if (_requestWebPackageHashOp == null)
                {
                    _requestWebPackageHashOp = new RequestWebServerPackageHashOperation(_fileSystem, _packageVersion, _timeout);
                    _requestWebPackageHashOp.StartOperation();
                    AddChildOperation(_requestWebPackageHashOp);
                }

                _requestWebPackageHashOp.UpdateOperation();
                if (_requestWebPackageHashOp.IsDone == false)
                    return;

                if (_requestWebPackageHashOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadWebPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_requestWebPackageHashOp.Error);
                }
            }

            if (_steps == ESteps.LoadWebPackageManifest)
            {
                if (_loadWebPackageManifestOp == null)
                {
                    string packageHash = _requestWebPackageHashOp.PackageHash;
                    _loadWebPackageManifestOp = new LoadWebServerPackageManifestOperation(_fileSystem, _packageVersion, packageHash, _timeout);
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