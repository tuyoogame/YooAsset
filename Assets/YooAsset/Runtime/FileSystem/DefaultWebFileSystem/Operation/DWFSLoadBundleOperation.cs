
namespace YooAsset
{
    internal class DWFSLoadAssetBundleOperation : FSLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            DownloadFile,
            CheckResult,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private DWFSDownloadWebFileOperation _downloadWebFileOp;
        private ESteps _steps = ESteps.None;


        internal DWFSLoadAssetBundleOperation(DefaultWebFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            DownloadProgress = 0f;
            DownloadedBytes = 0;
            _steps = ESteps.DownloadFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadWebFileOp == null)
                {
                    int failedTryAgain = int.MaxValue;
                    int timeout = 60;
                    string mainURL = _fileSystem.RemoteServices.GetRemoteMainURL(_bundle.FileName);
                    string fallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(_bundle.FileName);
                    _downloadWebFileOp = new DWFSDownloadWebFileOperation(_fileSystem, _bundle, mainURL, fallbackURL, failedTryAgain, timeout);
                }

                DownloadProgress = _downloadWebFileOp.DownloadProgress;
                DownloadedBytes = _downloadWebFileOp.DownloadedBytes;
                Progress = _downloadWebFileOp.Progress;
                if (_downloadWebFileOp.IsDone == false)
                    return;

                if (_downloadWebFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Result = _downloadWebFileOp.Result;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadWebFileOp.Error;
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            if (IsDone == false)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "WebGL platform not support sync load method !";
                UnityEngine.Debug.LogError(Error);
            }
        }
        public override void AbortDownloadOperation()
        {
            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadWebFileOp != null)
                    _downloadWebFileOp.SetAbort();
            }
        }
    }
}