#if UNITY_WEBGL && DOUYINMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

internal class TTFSLoadBundleOperation : FSLoadBundleOperation
{
    private enum ESteps
    {
        None,
        DownloadAssetBundle,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
    private readonly PackageBundle _bundle;
    private DownloadAssetBundleOperation _downloadAssetBundleOp;
    private ESteps _steps = ESteps.None;

    internal TTFSLoadBundleOperation(TiktokFileSystem fileSystem, PackageBundle bundle)
    {
        _fileSystem = fileSystem;
        _bundle = bundle;
    }
    internal override void InternalStart()
    {
        _steps = ESteps.DownloadAssetBundle;
    }
    internal override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.DownloadAssetBundle)
        {
            if (_downloadAssetBundleOp == null)
            {
                DownloadFileOptions options = new DownloadFileOptions(int.MaxValue, 60);
                options.MainURL = _fileSystem.RemoteServices.GetRemoteMainURL(_bundle.FileName); ;
                options.FallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(_bundle.FileName);

                if (_bundle.Encrypted)
                {
                    _downloadAssetBundleOp = new DownloadWebEncryptAssetBundleOperation(false, _fileSystem.DecryptionServices, _bundle, options);
                    _downloadAssetBundleOp.StartOperation();
                    AddChildOperation(_downloadAssetBundleOp);
                }
                else
                {
                    _downloadAssetBundleOp = new DownloadTiktokAssetBundleOperation(_bundle, options);
                    _downloadAssetBundleOp.StartOperation();
                    AddChildOperation(_downloadAssetBundleOp);
                }
            }

            _downloadAssetBundleOp.UpdateOperation();
            DownloadProgress = _downloadAssetBundleOp.DownloadProgress;
            DownloadedBytes = (long)_downloadAssetBundleOp.DownloadedBytes;
            Progress = DownloadProgress;
            if (_downloadAssetBundleOp.IsDone == false)
                return;

            if (_downloadAssetBundleOp.Status == EOperationStatus.Succeed)
            {
                var assetBundle = _downloadAssetBundleOp.Result;
                if (assetBundle == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(DownloadAssetBundleOperation)} loaded asset bundle is null !";
                }
                else
                {
                    _steps = ESteps.Done;
                    Result = new TTAssetBundleResult(_fileSystem, _bundle, assetBundle);
                    Status = EOperationStatus.Succeed;
                }
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _downloadAssetBundleOp.Error;
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