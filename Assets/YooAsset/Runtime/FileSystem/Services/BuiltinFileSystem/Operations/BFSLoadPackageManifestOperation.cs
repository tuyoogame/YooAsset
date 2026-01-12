
namespace YooAsset
{
    /// <summary>
    /// 内置文件系统的加载包裹清单操作
    /// </summary>
    internal sealed class BFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            RequestPackageHash,
            LoadPackageManifest,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly string _packageVersion;
        private RequestBuiltinPackageHashOperation _requestBuiltinPackageHashOp;
        private LoadBuiltinPackageManifestOperation _loadBuiltinPackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal BFSLoadPackageManifestOperation(BuiltinFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
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
                if (_requestBuiltinPackageHashOp == null)
                {
                    _requestBuiltinPackageHashOp = new RequestBuiltinPackageHashOperation(_fileSystem, _packageVersion);
                    _requestBuiltinPackageHashOp.StartOperation();
                    AddChildOperation(_requestBuiltinPackageHashOp);
                }

                _requestBuiltinPackageHashOp.UpdateOperation();
                if (_requestBuiltinPackageHashOp.IsDone == false)
                    return;

                if (_requestBuiltinPackageHashOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_requestBuiltinPackageHashOp.Error);
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
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

                if (_loadBuiltinPackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    Manifest = _loadBuiltinPackageManifestOp.Manifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadBuiltinPackageManifestOp.Error);
                }
            }
        }
    }
}