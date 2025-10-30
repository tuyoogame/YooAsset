
namespace YooAsset
{
    internal class DEFSLoadBundleOperation : FSLoadBundleOperation
    {
        protected enum ESteps
        {
            None,
            CheckExist,
            DownloadFile,
            AbortDownload,
            LoadAssetBundle,
            CheckResult,
            Done,
        }

        private readonly DefaultEditorFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        protected FSDownloadFileOperation _downloadFileOp;
        private int _asyncSimulateFrame;
        private ESteps _steps = ESteps.None;

        internal DEFSLoadBundleOperation(DefaultEditorFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckExist;
            _asyncSimulateFrame = _fileSystem.GetAsyncSimulateFrame();
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckExist)
            {
                if (_fileSystem.Exists(_bundle))
                {
                    DownloadProgress = 1f;
                    DownloadedBytes = _bundle.FileSize;
                    _steps = ESteps.LoadAssetBundle;
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                // 中断下载
                if (AbortDownloadFile)
                {
                    if (_downloadFileOp != null)
                        _downloadFileOp.AbortOperation();
                    _steps = ESteps.AbortDownload;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    DownloadFileOptions options = new DownloadFileOptions(int.MaxValue);
                    _downloadFileOp = _fileSystem.DownloadFileAsync(_bundle, options);
                    _downloadFileOp.StartOperation();
                    AddChildOperation(_downloadFileOp);
                }

                if (IsWaitForAsyncComplete)
                    _downloadFileOp.WaitForAsyncComplete();

                _downloadFileOp.UpdateOperation();
                DownloadProgress = _downloadFileOp.DownloadProgress;
                DownloadedBytes = _downloadFileOp.DownloadedBytes;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadAssetBundle;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadFileOp.Error;
                }
            }

            if (_steps == ESteps.AbortDownload)
            {
                if (_downloadFileOp != null)
                {
                    if (IsWaitForAsyncComplete)
                        _downloadFileOp.WaitForAsyncComplete();

                    _downloadFileOp.UpdateOperation();
                    if (_downloadFileOp.IsDone == false)
                        return;
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "Abort download file !";
            }

            if (_steps == ESteps.LoadAssetBundle)
            {
                if (IsWaitForAsyncComplete)
                {
                    if (_fileSystem.VirtualWebGLMode)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Virtual WebGL Mode only support asyn load method !";
                        YooLogger.Error(Error);
                    }
                    else
                    {
                        _steps = ESteps.CheckResult;
                    }
                }
                else
                {
                    if (_asyncSimulateFrame <= 0)
                        _steps = ESteps.CheckResult;
                    else
                        _asyncSimulateFrame--;
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                _steps = ESteps.Done;
                Result = new VirtualBundleResult(_fileSystem, _bundle);
                Status = EOperationStatus.Succeed;
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
    }
}