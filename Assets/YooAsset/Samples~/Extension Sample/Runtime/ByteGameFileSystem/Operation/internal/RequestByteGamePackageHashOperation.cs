#if UNITY_WEBGL && BYTEMINIGAME
using YooAsset;

internal class RequestByteGamePackageHashOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        RequestPackageHash,
        Done,
    }

    private readonly ByteGameFileSystem _fileSystem;
    private readonly string _packageVersion;
    private readonly int _timeout;
    private UnityWebTextRequestOperation _webTextRequestOp;
    private int _requestCount = 0;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 包裹哈希值
    /// </summary>
    public string PackageHash { private set; get; }


    public RequestByteGamePackageHashOperation(ByteGameFileSystem fileSystem, string packageVersion, int timeout)
    {
        _fileSystem = fileSystem;
        _packageVersion = packageVersion;
        _timeout = timeout;
    }
    internal override void InternalOnStart()
    {
        _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(RequestByteGamePackageHashOperation));
        _steps = ESteps.RequestPackageHash;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageHash)
        {
            if (_webTextRequestOp == null)
            {
                string fileName = YooAssetSettingsData.GetPackageHashFileName(_fileSystem.PackageName, _packageVersion);
                string url = GetRequestURL(fileName);
                _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
            }

            Progress = _webTextRequestOp.Progress;
            if (_webTextRequestOp.IsDone == false)
                return;

            if (_webTextRequestOp.Status == EOperationStatus.Succeed)
            {
                PackageHash = _webTextRequestOp.Result;
                if (string.IsNullOrEmpty(PackageHash))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Wechat package hash file content is empty !";
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
                WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(RequestByteGamePackageHashOperation));
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