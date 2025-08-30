#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

internal class TTFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
    private readonly bool _appendTimeTicks;
    private readonly int _timeout;
    private RequestWebPackageVersionOperation _requestPackageVersionOp;
    private ESteps _steps = ESteps.None;


    internal TTFSRequestPackageVersionOperation(TiktokFileSystem fileSystem, bool appendTimeTicks, int timeout)
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
            if (_requestPackageVersionOp == null)
            {
                _requestPackageVersionOp = new RequestWebPackageVersionOperation(_fileSystem.RemoteServices, _fileSystem.PackageName, _appendTimeTicks, _timeout);
                _requestPackageVersionOp.StartOperation();
                AddChildOperation(_requestPackageVersionOp);
            }

            _requestPackageVersionOp.UpdateOperation();
            Progress = _requestPackageVersionOp.Progress;
            if (_requestPackageVersionOp.IsDone == false)
                return;

            if (_requestPackageVersionOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                PackageVersion = _requestPackageVersionOp.PackageVersion;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _requestPackageVersionOp.Error;
            }
        }
    }
}
#endif