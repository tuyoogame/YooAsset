#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

internal class LoadTiktokPackageManifestOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        RequestFileData,
        VerifyFileData,
        LoadManifest,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
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


    internal LoadTiktokPackageManifestOperation(TiktokFileSystem fileSystem, string packageVersion, string packageHash, int timeout)
    {
        _fileSystem = fileSystem;
        _packageVersion = packageVersion;
        _packageHash = packageHash;
        _timeout = timeout;
    }
    internal override void InternalStart()
    {
        _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(LoadTiktokPackageManifestOperation));
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
                string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_fileSystem.PackageName, _packageVersion);
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
                WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(LoadTiktokPackageManifestOperation));
            }
        }

        if (_steps == ESteps.VerifyFileData)
        {
            string fileHash = HashUtility.BytesCRC32(_webDataRequestOp.Result);
            if (fileHash == _packageHash)
            {
                _steps = ESteps.LoadManifest;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Failed to verify package manifest file!";
            }
        }

        if (_steps == ESteps.LoadManifest)
        {
            if (_deserializer == null)
            {
                _deserializer = new DeserializeManifestOperation(_webDataRequestOp.Result);
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