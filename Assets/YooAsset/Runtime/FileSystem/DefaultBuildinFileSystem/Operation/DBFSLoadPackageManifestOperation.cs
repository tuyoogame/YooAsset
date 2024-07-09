
namespace YooAsset
{
    internal class DBFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestBuildinPackageHash,
            LoadBuildinPackageManifest,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly string _packageVersion;
        private RequestBuildinPackageHashOperation _requestBuildinPackageHashOp;
        private LoadBuildinPackageManifestOperation _loadBuildinPackageManifestOp;
        private ESteps _steps = ESteps.None;


        public DBFSLoadPackageManifestOperation(DefaultBuildinFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestBuildinPackageHash;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestBuildinPackageHash)
            {
                if (_requestBuildinPackageHashOp == null)
                {
                    _requestBuildinPackageHashOp = new RequestBuildinPackageHashOperation(_fileSystem, _packageVersion);
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
                    string packageHash = _requestBuildinPackageHashOp.PackageHash;
                    _loadBuildinPackageManifestOp = new LoadBuildinPackageManifestOperation(_fileSystem, _packageVersion, packageHash);
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