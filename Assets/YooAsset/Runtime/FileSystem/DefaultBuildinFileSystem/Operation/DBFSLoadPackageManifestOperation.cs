
namespace YooAsset
{
    internal class DBFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestBuildinPackageVersion,
            RequestBuildinPackageHash,
            LoadBuildinPackageManifest,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private RequestBuildinPackageVersionOperation _requestBuildinPackageVersionOp;
        private RequestBuildinPackageHashOperation _requestBuildinPackageHashOp;
        private LoadBuildinPackageManifestOperation _loadBuildinPackageManifestOp;
        private ESteps _steps = ESteps.None;


        public DBFSLoadPackageManifestOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestBuildinPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestBuildinPackageVersion)
            {
                if (_requestBuildinPackageVersionOp == null)
                {
                    _requestBuildinPackageVersionOp = new RequestBuildinPackageVersionOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestBuildinPackageVersionOp);
                }

                if (_requestBuildinPackageVersionOp.IsDone == false)
                    return;

                if (_requestBuildinPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.RequestBuildinPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuildinPackageVersionOp.Error;
                }
            }

            if (_steps == ESteps.RequestBuildinPackageHash)
            {
                if (_requestBuildinPackageHashOp == null)
                {
                    string packageVersion = _requestBuildinPackageVersionOp.PackageVersion;
                    _requestBuildinPackageHashOp = new RequestBuildinPackageHashOperation(_fileSystem, packageVersion);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestBuildinPackageHashOp);
                }

                if (_requestBuildinPackageHashOp.IsDone == false)
                    return;

                if (_requestBuildinPackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadBuildinPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuildinPackageHashOp.Error;
                }
            }

            if (_steps == ESteps.LoadBuildinPackageManifest)
            {
                if (_loadBuildinPackageManifestOp == null)
                {
                    string packageVersion = _requestBuildinPackageVersionOp.PackageVersion;
                    string packageHash = _requestBuildinPackageHashOp.PackageHash;
                    _loadBuildinPackageManifestOp = new LoadBuildinPackageManifestOperation(_fileSystem, packageVersion, packageHash);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadBuildinPackageManifestOp);
                }

                if (_loadBuildinPackageManifestOp.IsDone == false)
                    return;

                if (_loadBuildinPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadBuildinPackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadBuildinPackageManifestOp.Error;
                }
            }
        }
    }
}