
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件系统的查询包裹版本操作
    /// </summary>
    internal sealed class SFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            GetPackageVersion,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private RequestRemotePackageVersionOperation _requestRemotePackageVersionOp;
        private ESteps _steps = ESteps.None;


        internal SFSRequestPackageVersionOperation(SandboxFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.GetPackageVersion;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetPackageVersion)
            {
                if (_requestRemotePackageVersionOp == null)
                {
                    _requestRemotePackageVersionOp = new RequestRemotePackageVersionOperation(_fileSystem, _appendTimeTicks, _timeout);
                    _requestRemotePackageVersionOp.StartOperation();
                    AddChildOperation(_requestRemotePackageVersionOp);
                }

                _requestRemotePackageVersionOp.UpdateOperation();
                Progress = _requestRemotePackageVersionOp.Progress;
                if (_requestRemotePackageVersionOp.IsDone == false)
                    return;

                if (_requestRemotePackageVersionOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _requestRemotePackageVersionOp.PackageVersion;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_requestRemotePackageVersionOp.Error);
                }
            }
        }
    }
}