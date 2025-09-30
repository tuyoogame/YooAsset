
namespace YooAsset
{
    internal class DBFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly DefaultBuiltinFileSystem _fileSystem;
        private RequestBuiltinPackageVersionOperation _requestBuiltinPackageVersionOp;
        private ESteps _steps = ESteps.None;


        internal DBFSRequestPackageVersionOperation(DefaultBuiltinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_requestBuiltinPackageVersionOp == null)
                {
                    _requestBuiltinPackageVersionOp = new RequestBuiltinPackageVersionOperation(_fileSystem);
                    _requestBuiltinPackageVersionOp.StartOperation();
                    AddChildOperation(_requestBuiltinPackageVersionOp);
                }

                _requestBuiltinPackageVersionOp.UpdateOperation();
                if (_requestBuiltinPackageVersionOp.IsDone == false)
                    return;

                if (_requestBuiltinPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _requestBuiltinPackageVersionOp.PackageVersion;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestBuiltinPackageVersionOp.Error;
                }
            }
        }
    }
}