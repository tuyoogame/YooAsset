using UnityEngine;

namespace YooAsset
{
    internal class DownloadPackageBundleOperation : FSDownloadFileOperation
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
        protected readonly DefaultCacheFileSystem _fileSystem;
        protected readonly DownloadFileOptions _options;
        private UnityDownloadFileOperation _unityDownloadFileOp;

        protected int _requestCount = 0;
        protected float _tryAgainTimer = 0;
        protected int _failedTryAgain;
        private ESteps _steps = ESteps.None;


        internal DownloadPackageBundleOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, DownloadFileOptions options) : base(bundle)
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
                _unityDownloadFileOp = _fileSystem.DownloadCenter.DownloadFileAsync(Bundle, url);
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                if (IsWaitForAsyncComplete)
                    _unityDownloadFileOp.WaitForAsyncComplete();

                // 因为并发数量限制，下载器可能被挂起！
                if (_unityDownloadFileOp.Status == EOperationStatus.None)
                    return;

                _unityDownloadFileOp.UpdateOperation();
                Progress = _unityDownloadFileOp.Progress;
                DownloadedBytes = _unityDownloadFileOp.DownloadedBytes;
                DownloadProgress = _unityDownloadFileOp.DownloadProgress;
                if (_unityDownloadFileOp.IsDone == false)
                    return;

                if (_unityDownloadFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    if (IsWaitForAsyncComplete == false && _failedTryAgain > 0)
                    {
                        _steps = ESteps.TryAgain;
                        YooLogger.Warning($"Failed download : {_unityDownloadFileOp.URL} Try again !");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = _unityDownloadFileOp.Error;
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
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
        internal override void InternalAbort()
        {
            // 注意：取消下载任务的时候引用计数减一
            if (_steps != ESteps.Done)
            {
                if (_unityDownloadFileOp != null)
                {
                    _unityDownloadFileOp.Release();
                }
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