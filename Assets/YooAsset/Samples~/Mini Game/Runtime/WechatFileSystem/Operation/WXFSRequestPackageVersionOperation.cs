#if UNITY_WEBGL && WEIXINMINIGAME
using YooAsset;

internal class WXFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly bool _appendTimeTicks;
    private readonly int _timeout;
    private RequestWechatPackageVersionOperation _requestWebPackageVersionOp;
    private ESteps _steps = ESteps.None;


    internal WXFSRequestPackageVersionOperation(WechatFileSystem fileSystem, bool appendTimeTicks, int timeout)
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
                _requestWebPackageVersionOp = new RequestWechatPackageVersionOperation(_fileSystem, _appendTimeTicks, _timeout);
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