
namespace YooAsset
{
    internal class DWRFSLoadAssetBundleOperation : FSLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadWebAssetBundle,
            Done,
        }

        private readonly DefaultWebRemoteFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private LoadWebAssetBundleOperation _loadWebAssetBundleOp;
        private ESteps _steps = ESteps.None;


        internal DWRFSLoadAssetBundleOperation(DefaultWebRemoteFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.LoadWebAssetBundle;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadWebAssetBundle)
            {
                if (_loadWebAssetBundleOp == null)
                {
                    string mainURL = _fileSystem.RemoteServices.GetRemoteMainURL(_bundle.FileName);
                    string fallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(_bundle.FileName);
                    DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
                    options.SetURL(mainURL, fallbackURL);
                    
                    if (_bundle.Encrypted)
                    {
                        _loadWebAssetBundleOp = new LoadWebEncryptAssetBundleOperation(_bundle, options, _fileSystem.DecryptionServices);
                        _loadWebAssetBundleOp.StartOperation();
                        AddChildOperation(_loadWebAssetBundleOp);
                    }
                    else
                    {
                        _loadWebAssetBundleOp = new LoadWebNormalAssetBundleOperation(_bundle, options, _fileSystem.DisableUnityWebCache);
                        _loadWebAssetBundleOp.StartOperation();
                        AddChildOperation(_loadWebAssetBundleOp);
                    }
                }

                _loadWebAssetBundleOp.UpdateOperation();
                DownloadProgress = _loadWebAssetBundleOp.DownloadProgress;
                DownloadedBytes = _loadWebAssetBundleOp.DownloadedBytes;
                Progress = _loadWebAssetBundleOp.Progress;
                if (_loadWebAssetBundleOp.IsDone == false)
                    return;

                if (_loadWebAssetBundleOp.Status == EOperationStatus.Succeed)
                {
                    var assetBundle = _loadWebAssetBundleOp.Result;
                    if (assetBundle == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"{nameof(DWRFSLoadAssetBundleOperation)} loaded asset bundle is null !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Result = new AssetBundleResult(_fileSystem, _bundle, assetBundle, null);
                        Status = EOperationStatus.Succeed;
                    }
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
}