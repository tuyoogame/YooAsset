using System.IO;

namespace YooAsset
{
    internal sealed class DownloadAndCacheRemoteFileOperation : DownloadAndCacheFileOperation
    {
        private enum ESteps
        {
            None,
            CreateRequest,
            CheckRequest,
            VerifyBundleFile,
            CacheBundleFile,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private readonly string _tempFilePath;
        private bool _enableResume = false;
        private long _fileOriginLength = 0;
        private IDownloadRequest _request;
        private VerifyTempFileOperation _verifyOperation;
        private ESteps _steps = ESteps.None;

        internal DownloadAndCacheRemoteFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string url) : base(url)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
            _tempFilePath = _fileSystem.GetTempFilePath(_bundle);
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载请求
            if (_steps == ESteps.CreateRequest)
            {
                FileUtility.CreateFileDirectory(_tempFilePath);

                _enableResume = _bundle.FileSize >= _fileSystem.ResumeDownloadMinimumSize;
                if (_enableResume)
                {
                    _request = CreateResumeRequest();
                    _request.SendRequest();
                    _steps = ESteps.CheckRequest;
                }
                else
                {
                    _request = CreateNormalRequest();
                    _request.SendRequest();
                    _steps = ESteps.CheckRequest;
                }
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                DownloadProgress = _request.DownloadProgress;
                DownloadedBytes = _fileOriginLength + _request.DownloadedBytes;
                Progress = DownloadProgress;
                if (_request.IsDone == false)
                    return;

                // 检查网络错误
                if (_request.Status == EDownloadRequestStatus.Succeed)
                {
                    _steps = ESteps.VerifyBundleFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _request.Error;
                }

                // 在遇到特殊错误的时候删除文件
                if (_enableResume)
                    ClearTempFileWhenError(_request.HttpCode);

                // 最终释放请求器
                _request.Dispose();
            }

            // 验证下载结果
            if (_steps == ESteps.VerifyBundleFile)
            {
                if (_verifyOperation == null)
                {
                    var element = new TempFileElement(_tempFilePath, _bundle.FileCRC, _bundle.FileSize);
                    _verifyOperation = new VerifyTempFileOperation(element);
                    _verifyOperation.StartOperation();
                    AddChildOperation(_verifyOperation);
                }

                if (IsWaitForAsyncComplete)
                    _verifyOperation.WaitForAsyncComplete();

                _verifyOperation.UpdateOperation();
                if (_verifyOperation.IsDone == false)
                    return;

                if (_verifyOperation.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CacheBundleFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _verifyOperation.Error;

                    // 注意：验证失败后直接删除文件
                    if (File.Exists(_tempFilePath))
                        File.Delete(_tempFilePath);
                }
            }

            // 缓存文件
            if (_steps == ESteps.CacheBundleFile)
            {
                if (_fileSystem.WriteCacheBundleFile(_bundle, _tempFilePath))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{_fileSystem.GetType().FullName} failed to write file !";
                }

                // 注意：缓存完成后直接删除临时文件
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }
        }
        internal override void InternalAbort()
        {
            if (_request != null)
                _request.AbortRequest();
        }
        internal override void InternalWaitForAsyncComplete()
        {
            if (_steps != ESteps.Done)
            {
                // 注意：不中断下载任务，保持后台继续下载
                YooLogger.Error($"Try load bundle {_bundle.BundleName} from remote : {URL} !");
            }
        }

        private IDownloadRequest CreateResumeRequest()
        {
            // 获取下载起始位置
            if (File.Exists(_tempFilePath))
            {
                FileInfo fileInfo = new FileInfo(_tempFilePath);
                if (fileInfo.Length >= _bundle.FileSize)
                {
                    File.Delete(_tempFilePath);
                }
                else
                {
                    _fileOriginLength = fileInfo.Length;
                }
            }

            int watchdogTime = _fileSystem.DownloadWatchDogTime;
            int timeout = 0; //注意：文件下载不做超时检测
            bool appendToFile = true;
            bool removeFileOnAbort = false;
            long resumeFromBytes = _fileOriginLength;
            var args = new DownloadFileRequestArgs(URL, _tempFilePath, timeout, watchdogTime, appendToFile, removeFileOnAbort, resumeFromBytes);
            return _fileSystem.DownloadBackend.CreateFileRequest(args);
        }
        private IDownloadRequest CreateNormalRequest()
        {
            // 删除历史缓存文件
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);

            int watchdogTime = _fileSystem.DownloadWatchDogTime;
            int timeout = 0; //注意：文件下载不做超时检测
            var args = new DownloadFileRequestArgs(URL, _tempFilePath, timeout, watchdogTime);
            return _fileSystem.DownloadBackend.CreateFileRequest(args);
        }
        private void ClearTempFileWhenError(long httpCode)
        {
            if (_fileSystem.ResumeDownloadResponseCodes == null)
                return;

            //说明：如果遇到以下错误返回码，验证失败直接删除文件
            if (_fileSystem.ResumeDownloadResponseCodes.Contains(httpCode))
            {
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }
        }
    }
}