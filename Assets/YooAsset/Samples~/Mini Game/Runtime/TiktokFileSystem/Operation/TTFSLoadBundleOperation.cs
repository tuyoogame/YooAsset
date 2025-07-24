#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

internal class TTFSLoadBundleOperation : FSLoadBundleOperation
{
    private enum ESteps
    {
        None,
        LoadAssetBundle,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
    private readonly PackageBundle _bundle;
    private LoadWebAssetBundleOperation _loadWebAssetBundleOp;
    private ESteps _steps = ESteps.None;

    internal TTFSLoadBundleOperation(TiktokFileSystem fileSystem, PackageBundle bundle)
    {
        _fileSystem = fileSystem;
        _bundle = bundle;
    }
    internal override void InternalStart()
    {
        _steps = ESteps.LoadAssetBundle;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadAssetBundle)
        {
            if (_loadWebAssetBundleOp == null)
            {
                string mainRUL = _fileSystem.RemoteServices.GetRemoteMainURL(_bundle.FileName);
                string fallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(_bundle.FileName);
                DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
                options.SetURL(mainRUL, fallbackURL);

                if (_bundle.Encrypted)
                {
                    _loadWebAssetBundleOp = new LoadWebEncryptAssetBundleOperation(_bundle, options, _fileSystem.DecryptionServices);
                    _loadWebAssetBundleOp.StartOperation();
                    AddChildOperation(_loadWebAssetBundleOp);
                }
                else
                {
                    _loadWebAssetBundleOp = new LoadTiktokAssetBundleOperation(_bundle, options);
                    _loadWebAssetBundleOp.StartOperation();
                    AddChildOperation(_loadWebAssetBundleOp);
                }
            }

            _loadWebAssetBundleOp.UpdateOperation();
            Progress = _loadWebAssetBundleOp.Progress;
            DownloadProgress = _loadWebAssetBundleOp.DownloadProgress;
            DownloadedBytes = _loadWebAssetBundleOp.DownloadedBytes;
            if (_loadWebAssetBundleOp.IsDone == false)
                return;

            if (_loadWebAssetBundleOp.Status == EOperationStatus.Succeed)
            {
                var assetBundle = _loadWebAssetBundleOp.Result;
                _steps = ESteps.Done;
                Result = new TTAssetBundleResult(_fileSystem, _bundle, assetBundle);
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _loadWebAssetBundleOp.Error;
            }
        }
    }
    internal override void InternalWaitForAsyncComplete()
    {
        if (_steps != ESteps.Done)
        {
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = "WebGL platform not support sync load method !";
            UnityEngine.Debug.LogError(Error);
        }
    }
}
#endif