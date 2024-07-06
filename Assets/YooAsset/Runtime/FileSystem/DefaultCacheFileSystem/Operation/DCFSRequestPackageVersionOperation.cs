
namespace YooAsset
{
    internal class DCFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            GetPackageVersion,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private RequestRemotePackageVersionOperation _requestRemotePackageVersionOp;
        private ESteps _steps = ESteps.None;


        internal DCFSRequestPackageVersionOperation(DefaultCacheFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.GetPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetPackageVersion)
            {
                if (_requestRemotePackageVersionOp == null)
                {
                    _requestRemotePackageVersionOp = new RequestRemotePackageVersionOperation(_fileSystem, _appendTimeTicks, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestRemotePackageVersionOp);
                }

                Progress = _requestRemotePackageVersionOp.Progress;
                if (_requestRemotePackageVersionOp.IsDone == false)
                    return;

                if (_requestRemotePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _requestRemotePackageVersionOp.PackageVersion;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestRemotePackageVersionOp.Error;
                }
            }
        }
    }
}