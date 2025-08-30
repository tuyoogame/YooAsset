using YooAsset;

internal class LoadWebPackageManifestOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        RequestFileData,
        VerifyFileData,
        LoadManifest,
        Done,
    }

    private readonly IManifestRestoreServices _manifestServices;
    private readonly IRemoteServices _remoteServices;
    private readonly string _packageName;
    private readonly string _packageVersion;
    private readonly string _packageHash;
    private readonly int _timeout;
    private UnityWebDataRequestOperation _webDataRequestOp;
    private DeserializeManifestOperation _deserializer;
    private int _requestCount = 0;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 包裹清单
    /// </summary>
    public PackageManifest Manifest { private set; get; }


    internal LoadWebPackageManifestOperation(IManifestRestoreServices manifestServices, IRemoteServices remoteServices,
        string packageName, string packageVersion, string packageHash, int timeout)
    {
        _manifestServices = manifestServices;
        _remoteServices = remoteServices;
        _packageName = packageName;
        _packageVersion = packageVersion;
        _packageHash = packageHash;
        _timeout = timeout;
    }
    internal override void InternalStart()
    {
        _requestCount = WebRequestCounter.GetRequestFailedCount(_packageName, nameof(LoadWebPackageManifestOperation));
        _steps = ESteps.RequestFileData;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestFileData)
        {
            if (_webDataRequestOp == null)
            {
                string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, _packageVersion);
                string url = GetRequestURL(fileName);
                _webDataRequestOp = new UnityWebDataRequestOperation(url, _timeout);
                _webDataRequestOp.StartOperation();
                AddChildOperation(_webDataRequestOp);
            }

            _webDataRequestOp.UpdateOperation();
            Progress = _webDataRequestOp.Progress;
            if (_webDataRequestOp.IsDone == false)
                return;

            if (_webDataRequestOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.VerifyFileData;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _webDataRequestOp.Error;
                WebRequestCounter.RecordRequestFailed(_packageName, nameof(LoadWebPackageManifestOperation));
            }
        }

        if (_steps == ESteps.VerifyFileData)
        {
            if (ManifestTools.VerifyManifestData(_webDataRequestOp.Result, _packageHash))
            {
                _steps = ESteps.LoadManifest;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Failed to verify web package manifest file!";
            }
        }

        if (_steps == ESteps.LoadManifest)
        {
            if (_deserializer == null)
            {
                _deserializer = new DeserializeManifestOperation(_manifestServices, _webDataRequestOp.Result);
                _deserializer.StartOperation();
                AddChildOperation(_deserializer);
            }

            _deserializer.UpdateOperation();
            Progress = _deserializer.Progress;
            if (_deserializer.IsDone == false)
                return;

            if (_deserializer.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                Manifest = _deserializer.Manifest;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _deserializer.Error;
            }
        }
    }
    internal override string InternalGetDesc()
    {
        return $"PackageVersion : {_packageVersion} PackageHash : {_packageHash}";
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