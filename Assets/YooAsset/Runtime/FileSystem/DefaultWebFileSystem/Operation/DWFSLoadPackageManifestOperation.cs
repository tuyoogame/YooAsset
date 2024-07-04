
namespace YooAsset
{
    internal class DWFSLoadWebPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestWebPackageVersion,
            RequestWebPackageHash,
            LoadWebPackageManifest,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly int _timeout;
        private RequestWebPackageVersionOperation _requestWebPackageVersionOp;
        private RequestWebPackageHashOperation _requestWebPackageHashOp;
        private LoadWebPackageManifestOperation _loadWebPackageManifestOp;
        private ESteps _steps = ESteps.None;


        public DWFSLoadWebPackageManifestOperation(DefaultWebFileSystem fileSystem, int timeout)
        {
            _fileSystem = fileSystem;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestWebPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestWebPackageVersion)
            {
                if (_requestWebPackageVersionOp == null)
                {
                    _requestWebPackageVersionOp = new RequestWebPackageVersionOperation(_fileSystem, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestWebPackageVersionOp);
                }

                if (_requestWebPackageVersionOp.IsDone == false)
                    return;

                if (_requestWebPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.RequestWebPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestWebPackageVersionOp.Error;
                }
            }

            if (_steps == ESteps.RequestWebPackageHash)
            {
                if (_requestWebPackageHashOp == null)
                {
                    string packageVersion = _requestWebPackageVersionOp.PackageVersion;
                    _requestWebPackageHashOp = new RequestWebPackageHashOperation(_fileSystem, packageVersion, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestWebPackageHashOp);
                }

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
                    string packageVersion = _requestWebPackageVersionOp.PackageVersion;
                    string packageHash = _requestWebPackageHashOp.PackageHash;
                    _loadWebPackageManifestOp = new LoadWebPackageManifestOperation(_fileSystem, packageVersion, packageHash);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadWebPackageManifestOp);
                }

                Progress = _loadWebPackageManifestOp.Progress;
                if (_loadWebPackageManifestOp.IsDone == false)
                    return;

                if (_loadWebPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Result = _loadWebPackageManifestOp.Manifest;
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

    internal class DWFSLoadRemotePackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestRemotePackageHash,
            LoadRemotePackageManifest,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private RequestRemotePackageHashOperation _requestRemotePackageHashOp;
        private LoadRemotePackageManifestOperation _loadRemotePackageManifestOp;
        private ESteps _steps = ESteps.None;


        public DWFSLoadRemotePackageManifestOperation(DefaultWebFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestRemotePackageHash;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestRemotePackageHash)
            {
                if (_requestRemotePackageHashOp == null)
                {
                    _requestRemotePackageHashOp = new RequestRemotePackageHashOperation(_fileSystem, _packageVersion, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestRemotePackageHashOp);
                }

                if (_requestRemotePackageHashOp.IsDone == false)
                    return;

                if (_requestRemotePackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadRemotePackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestRemotePackageHashOp.Error;
                }
            }

            if (_steps == ESteps.LoadRemotePackageManifest)
            {
                if (_loadRemotePackageManifestOp == null)
                {
                    string packageHash = _requestRemotePackageHashOp.PackageHash;
                    _loadRemotePackageManifestOp = new LoadRemotePackageManifestOperation(_fileSystem, _packageVersion, packageHash, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadRemotePackageManifestOp);
                }

                Progress = _loadRemotePackageManifestOp.Progress;
                if (_loadRemotePackageManifestOp.IsDone == false)
                    return;

                if (_loadRemotePackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Result = _loadRemotePackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadRemotePackageManifestOp.Error;
                }
            }
        }
    }
}