#if UNITY_WEBGL && TAPMINIGAME
using YooAsset;

internal class TPFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly TaptapFileSystem _fileSystem;
    private readonly bool _appendTimeTicks;
    private readonly int _timeout;
    private RequestWebPackageVersionOperation _requestWebPackageVersionOp;
    private ESteps _steps = ESteps.None;


    internal TPFSRequestPackageVersionOperation(TaptapFileSystem fileSystem, bool appendTimeTicks, int timeout)
    {
        _fileSystem = fileSystem;
        _appendTimeTicks = appendTimeTicks;
        _timeout = timeout;
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
            if (_requestWebPackageVersionOp == null)
            {
                _requestWebPackageVersionOp = new RequestWebPackageVersionOperation(_fileSystem.RemoteServices, _fileSystem.PackageName, _appendTimeTicks, _timeout);
                _requestWebPackageVersionOp.StartOperation();
                AddChildOperation(_requestWebPackageVersionOp);
            }

            _requestWebPackageVersionOp.UpdateOperation();
            Progress = _requestWebPackageVersionOp.Progress;
            if (_requestWebPackageVersionOp.IsDone == false)
                return;

            if (_requestWebPackageVersionOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                PackageVersion = _requestWebPackageVersionOp.PackageVersion;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _requestWebPackageVersionOp.Error;
            }
        }
    }
}
#endif