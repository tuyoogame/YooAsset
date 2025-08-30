using YooAsset;

internal class RequestWebPackageVersionOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly IRemoteServices _remoteServices;
    private readonly string _packageName;
    private readonly bool _appendTimeTicks;
    private readonly int _timeout;
    private UnityWebTextRequestOperation _webTextRequestOp;
    private int _requestCount = 0;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 包裹版本
    /// </summary>
    public string PackageVersion { private set; get; }


    public RequestWebPackageVersionOperation(IRemoteServices remoteServices, string packageName, bool appendTimeTicks, int timeout)
    {
        _remoteServices = remoteServices;
        _packageName = packageName;
        _appendTimeTicks = appendTimeTicks;
        _timeout = timeout;
    }
    internal override void InternalStart()
    {
        _requestCount = WebRequestCounter.GetRequestFailedCount(_packageName, nameof(RequestWebPackageVersionOperation));
        _steps = ESteps.RequestPackageVersion;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageVersion)
        {
            if (_webTextRequestOp == null)
            {
                string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
                string url = GetRequestURL(fileName);
                _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                _webTextRequestOp.StartOperation();
                AddChildOperation(_webTextRequestOp);
            }

            _webTextRequestOp.UpdateOperation();
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
                    Error = $"Web package version file content is empty !";
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
                WebRequestCounter.RecordRequestFailed(_packageName, nameof(RequestWebPackageVersionOperation));
            }
        }
    }

    private string GetRequestURL(string fileName)
    {
        string url;

        // 轮流返回请求地址
        if (_requestCount % 2 == 0)
            url = _remoteServices.GetRemoteMainURL(fileName);
        else
            url = _remoteServices.GetRemoteFallbackURL(fileName);

        // 在URL末尾添加时间戳
        if (_appendTimeTicks)
            return $"{url}?{System.DateTime.UtcNow.Ticks}";
        else
            return url;
    }
}