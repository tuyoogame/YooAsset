using YooAsset;

internal class RequestWebPackageHashOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        RequestPackageHash,
        Done,
    }

    private readonly IRemoteServices _remoteServices;
    private readonly IDownloadBackend _downloadBackend;
    private readonly string _packageName;
    private readonly string _packageVersion;
    private readonly int _timeout;
    private IDownloadTextRequest _webTextRequestOp;
    private int _requestCount = 0;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 包裹哈希值
    /// </summary>
    public string PackageHash { private set; get; }


    public RequestWebPackageHashOperation(IRemoteServices remoteServices, IDownloadBackend downloadBackend, string packageName, string packageVersion, int timeout)
    {
        _remoteServices = remoteServices;
        _downloadBackend = downloadBackend;
        _packageName = packageName;
        _packageVersion = packageVersion;
        _timeout = timeout;
    }
    internal override void InternalStart()
    {
        _requestCount = WebRequestCounter.GetRequestFailedCount(_packageName, nameof(RequestWebPackageHashOperation));
        _steps = ESteps.RequestPackageHash;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageHash)
        {
            if (_webTextRequestOp == null)
            {
                string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, _packageVersion);
                string url = GetRequestURL(fileName);
                var args = new DownloadDataRequestArgs(url, _timeout, 0);
                _webTextRequestOp = _downloadBackend.CreateTextRequest(args);
                _webTextRequestOp.SendRequest();
            }

            Progress = _webTextRequestOp.DownloadProgress;
            if (_webTextRequestOp.IsDone == false)
                return;

            if (_webTextRequestOp.Status == EDownloadRequestStatus.Succeed)
            {
                PackageHash = _webTextRequestOp.Result;
                if (string.IsNullOrEmpty(PackageHash))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Web package hash file content is empty !";
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
                WebRequestCounter.RecordRequestFailed(_packageName, nameof(RequestWebPackageHashOperation));
            }
        }
    }

    private string GetRequestURL(string fileName)
    {
        // 轮流返回请求地址
        if (_requestCount % 2 == 0)
            return _remoteServices.GetRemoteMainURL(fileName);
        else
            return _remoteServices.GetRemoteFallbackURL(fileName);
    }
}