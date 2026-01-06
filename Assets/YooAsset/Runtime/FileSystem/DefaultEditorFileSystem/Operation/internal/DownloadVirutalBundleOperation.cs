using UnityEngine;

namespace YooAsset
{
    internal class DownloadVirtualBundleOperation : FSDownloadFileOperation
    {
        protected enum ESteps
        {
            None,
            CheckExists,
            CreateRequest,
            CheckRequest,
            TryAgain,
            Done,
        }

        // 下载参数
        protected readonly DefaultEditorFileSystem _fileSystem;
        protected readonly DownloadFileOptions _options;
        protected IDownloadFileRequest _downloadFileOp;

        protected int _requestCount = 0;
        protected float _tryAgainTimer = 0;
        protected int _failedTryAgain;
        private ESteps _steps = ESteps.None;


        internal DownloadVirtualBundleOperation(DefaultEditorFileSystem fileSystem, PackageBundle bundle, DownloadFileOptions options) : base(bundle)
        {
            _fileSystem = fileSystem;
            _options = options;
            _failedTryAgain = options.FailedTryAgain;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CheckExists;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 检测文件是否存在
            if (_steps == ESteps.CheckExists)
            {
                if (_fileSystem.Exists(Bundle))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.CreateRequest;
                }
            }

            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                if (_options.IsValid() == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Download file options is invalid !";
                    Debug.Log(Error);
                    return;
                }

                string url = GetRequestURL();
                int speed = _fileSystem.VirtualDownloadSpeed;
                var args = new DownloadSimulateRequestArgs(url, Bundle.FileSize, speed);
                _downloadFileOp = _fileSystem.DownloadBackend.CreateSimulateRequest(args);
                _downloadFileOp.SendRequest();
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                Progress = _downloadFileOp.DownloadProgress;
                DownloadedBytes = _downloadFileOp.DownloadedBytes;
                DownloadProgress = _downloadFileOp.DownloadProgress;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EDownloadRequestStatus.Succeed)
                {
                    _fileSystem.RecordDownloadFile(Bundle);
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    if (IsWaitForAsyncComplete == false && _failedTryAgain > 0)
                    {
                        _steps = ESteps.TryAgain;
                        YooLogger.Warning($"Failed download : {_downloadFileOp.URL} Try again !");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = _downloadFileOp.Error;
                        YooLogger.Error(Error);
                    }
                }
            }

            // 重新尝试下载
            if (_steps == ESteps.TryAgain)
            {
                _tryAgainTimer += Time.unscaledDeltaTime;
                if (_tryAgainTimer > 1f)
                {
                    _tryAgainTimer = 0f;
                    _failedTryAgain--;
                    Progress = 0f;
                    DownloadProgress = 0f;
                    DownloadedBytes = 0;
                    _steps = ESteps.CreateRequest;
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            if (_steps != ESteps.Done)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = $"Try load bundle {Bundle.BundleName} from remote !";
                YooLogger.Error(Error);
            }
        }

        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            _requestCount++;
            if (_requestCount % 2 == 0)
                return _options.FallbackURL;
            else
                return _options.MainURL;
        }
    }
}