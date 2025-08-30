
namespace YooAsset
{
    internal class DWRFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestWebPackageHash,
            LoadWebPackageManifest,
            Done,
        }

        private readonly DefaultWebRemoteFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private RequestWebPackageHashOperation _requestWebPackageHashOp;
        private LoadWebPackageManifestOperation _loadWebPackageManifestOp;
        private ESteps _steps = ESteps.None;


        public DWRFSLoadPackageManifestOperation(DefaultWebRemoteFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestWebPackageHash;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestWebPackageHash)
            {
                if (_requestWebPackageHashOp == null)
                {
                    _requestWebPackageHashOp = new RequestWebPackageHashOperation(_fileSystem.RemoteServices, _fileSystem.PackageName, _packageVersion, _timeout);
                    _requestWebPackageHashOp.StartOperation();
                    AddChildOperation(_requestWebPackageHashOp);
                }

                _requestWebPackageHashOp.UpdateOperation();
                if (_requestWebPackageHashOp.IsDone == false)
                    return;

                if (_requestWebPackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadWebPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestWebPackageHashOp.Error;
                }
            }

            if (_steps == ESteps.LoadWebPackageManifest)
            {
                if (_loadWebPackageManifestOp == null)
                {
                    string packageHash = _requestWebPackageHashOp.PackageHash;
                    string packageName = _fileSystem.PackageName;
                    var manifestServices = _fileSystem.ManifestServices;
                    var remoteServices = _fileSystem.RemoteServices;
                    _loadWebPackageManifestOp = new LoadWebPackageManifestOperation(manifestServices, remoteServices, packageName, _packageVersion, packageHash, _timeout);
                    _loadWebPackageManifestOp.StartOperation();
                    AddChildOperation(_loadWebPackageManifestOp);
                }

                _loadWebPackageManifestOp.UpdateOperation();
                Progress = _loadWebPackageManifestOp.Progress;
                if (_loadWebPackageManifestOp.IsDone == false)
                    return;

                if (_loadWebPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadWebPackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadWebPackageManifestOp.Error;
                }
            }
        }
    }
}