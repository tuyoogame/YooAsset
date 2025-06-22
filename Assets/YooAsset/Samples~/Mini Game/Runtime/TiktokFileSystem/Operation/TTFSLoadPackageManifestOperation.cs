#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

internal class TTFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
{
    private enum ESteps
    {
        None,
        RequestPackageHash,
        LoadPackageManifest,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
    private readonly string _packageVersion;
    private readonly int _timeout;
    private RequestTiktokPackageHashOperation _requestPackageHashOp;
    private LoadTiktokPackageManifestOperation _loadPackageManifestOp;
    private ESteps _steps = ESteps.None;

    
    public TTFSLoadPackageManifestOperation(TiktokFileSystem fileSystem, string packageVersion, int timeout)
    {
        _fileSystem = fileSystem;
        _packageVersion = packageVersion;
        _timeout = timeout;
    }
    internal override void InternalStart()
    {
        _steps = ESteps.RequestPackageHash;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageHash)
        {
            if (_requestPackageHashOp == null)
            {
                _requestPackageHashOp = new RequestTiktokPackageHashOperation(_fileSystem, _packageVersion, _timeout);
                _requestPackageHashOp.StartOperation();
                AddChildOperation(_requestPackageHashOp);
            }

            _requestPackageHashOp.UpdateOperation();
            if (_requestPackageHashOp.IsDone == false)
                return;

            if (_requestPackageHashOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.LoadPackageManifest;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _requestPackageHashOp.Error;
            }
        }

        if (_steps == ESteps.LoadPackageManifest)
        {
            if (_loadPackageManifestOp == null)
            {
                string packageHash = _requestPackageHashOp.PackageHash;
                _loadPackageManifestOp = new LoadTiktokPackageManifestOperation(_fileSystem, _packageVersion, packageHash, _timeout);
                _loadPackageManifestOp.StartOperation();
                AddChildOperation(_loadPackageManifestOp);
            }

            _loadPackageManifestOp.UpdateOperation();
            Progress = _loadPackageManifestOp.Progress;
            if (_loadPackageManifestOp.IsDone == false)
                return;

            if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                Manifest = _loadPackageManifestOp.Manifest;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _loadPackageManifestOp.Error;
            }
        }
    }
}
#endif