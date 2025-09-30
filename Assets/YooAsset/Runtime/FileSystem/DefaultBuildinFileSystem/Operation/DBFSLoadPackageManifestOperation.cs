
namespace YooAsset
{
    internal class DBFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestBuiltinPackageHash,
            LoadBuiltinPackageManifest,
            Done,
        }

        private readonly DefaultBuiltinFileSystem _fileSystem;
        private readonly string _packageVersion;
        private RequestBuiltinPackageHashOperation _requestBuiltinPackageHashOp;
        private LoadBuiltinPackageManifestOperation _loadBuiltinPackageManifestOp;
        private ESteps _steps = ESteps.None;


        public DBFSLoadPackageManifestOperation(DefaultBuiltinFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestBuiltinPackageHash;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestBuiltinPackageHash)
            {
                if (_requestBuiltinPackageHashOp == null)
                {
                    _requestBuiltinPackageHashOp = new RequestBuiltinPackageHashOperation(_fileSystem, _packageVersion);
                    _requestBuiltinPackageHashOp.StartOperation();
                    AddChildOperation(_requestBuiltinPackageHashOp);
                }

                _requestBuiltinPackageHashOp.UpdateOperation();
                if (_requestBuiltinPackageHashOp.IsDone == false)
                    return;

                if (_requestBuiltinPackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadBuiltinPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuiltinPackageHashOp.Error;
                }
            }

            if (_steps == ESteps.LoadBuiltinPackageManifest)
            {
                if (_loadBuiltinPackageManifestOp == null)
                {
                    string packageHash = _requestBuiltinPackageHashOp.PackageHash;
                    _loadBuiltinPackageManifestOp = new LoadBuiltinPackageManifestOperation(_fileSystem, _packageVersion, packageHash);
                    _loadBuiltinPackageManifestOp.StartOperation();
                    AddChildOperation(_loadBuiltinPackageManifestOp);
                }

                _loadBuiltinPackageManifestOp.UpdateOperation();
                if (_loadBuiltinPackageManifestOp.IsDone == false)
                    return;

                if (_loadBuiltinPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadBuiltinPackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadBuiltinPackageManifestOp.Error;
                }
            }
        }
    }
}