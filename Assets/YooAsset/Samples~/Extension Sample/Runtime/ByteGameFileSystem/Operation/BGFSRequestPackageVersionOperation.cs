#if UNITY_WEBGL && BYTEMINIGAME
using YooAsset;

internal class BGFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly ByteGameFileSystem _fileSystem;
    private readonly int _timeout;
    private RequestByteGamePackageVersionOperation _requestWebPackageVersionOp;
    private ESteps _steps = ESteps.None;


    internal BGFSRequestPackageVersionOperation(ByteGameFileSystem fileSystem, int timeout)
    {
        _fileSystem = fileSystem;
        _timeout = timeout;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.RequestPackageVersion;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageVersion)
        {
            if (_requestWebPackageVersionOp == null)
            {
                _requestWebPackageVersionOp = new RequestByteGamePackageVersionOperation(_fileSystem, _timeout);
                OperationSystem.StartOperation(_fileSystem.PackageName, _requestWebPackageVersionOp);
            }

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