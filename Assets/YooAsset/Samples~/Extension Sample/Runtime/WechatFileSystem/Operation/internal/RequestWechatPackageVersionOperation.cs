#if UNITY_WEBGL && WEIXINMINIGAME
using YooAsset;

internal class RequestWechatPackageVersionOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly int _timeout;
    private UnityWebTextRequestOperation _webTextRequestOp;
    private int _requestCount = 0;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 包裹版本
    /// </summary>
    public string PackageVersion { private set; get; }

    
    public RequestWechatPackageVersionOperation(WechatFileSystem fileSystem, int timeout)
    {
        _fileSystem = fileSystem;
        _timeout = timeout;
    }
    internal override void InternalOnStart()
    {
        _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(RequestWechatPackageVersionOperation));
        _steps = ESteps.RequestPackageVersion;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageVersion)
        {
            if (_webTextRequestOp == null)
            {
                string fileName = YooAssetSettingsData.GetPackageVersionFileName(_fileSystem.PackageName);
                string url = GetRequestURL(fileName);
                _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
            }

            Progress = _webTextRequestOp.Progress;
            if (_webTextRequestOp.IsDone == false)
                return;

            if (_webTextRequestOp.Status == EOperationStatus.Succeed)
            {
                PackageVersion = _webTextRequestOp.Result;
                if (string.IsNullOrEmpty(PackageVersion))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Wechat package version file content is empty !";
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _webTextRequestOp.Error;
                WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(RequestWechatPackageVersionOperation));
            }
        }
    }

    private string GetRequestURL(string fileName)
    {
        // 轮流返回请求地址
        if (_requestCount % 2 == 0)
            return _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
        else
            return _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);
    }
}
#endif