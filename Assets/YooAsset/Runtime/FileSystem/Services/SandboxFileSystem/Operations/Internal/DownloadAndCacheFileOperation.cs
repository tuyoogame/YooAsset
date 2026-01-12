using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 下载远端资源并缓存到本地的操作
    /// </summary>
    /// <remarks>
    /// TODO: 下载和缓存不能拆分，因为FSDownloadBundleOperation下载任务并不唯一，会造成写入缓存冲突。
    /// </remarks>
    internal sealed class DownloadAndCacheFileOperation : DownloadFileBaseOperation
    {
        private enum ESteps
        {
            None,
            CreateRequest,
            CheckRequest,
            CacheFile,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly string _tempFilePath;
        private IDownloadFileRequest _downloadFileRequest;
        private BCWriteCacheOperation _writeCacheOp;
        private bool _enableResume;
        private long _fileOriginLength = 0;
        private ESteps _steps = ESteps.None;

        internal DownloadAndCacheFileOperation(SandboxFileSystem fileSystem, PackageBundle bundle, string url) : base(bundle, url)
        {
            _fileSystem = fileSystem;
            _tempFilePath = _fileSystem.GetTempFilePath(bundle);
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CreateRequest;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载请求
            if (_steps == ESteps.CreateRequest)
            {
                FileUtility.EnsureParentDirectoryExists(_tempFilePath);
                _enableResume = Bundle.FileSize >= _fileSystem.ResumeDownloadMinimumSize;
                if (_enableResume)
                {
                    _downloadFileRequest = CreateResumeRequest();
                    _downloadFileRequest.SendRequest();
                    _steps = ESteps.CheckRequest;
                }
                else
                {
                    _downloadFileRequest = CreateNormalRequest();
                    _downloadFileRequest.SendRequest();
                    _steps = ESteps.CheckRequest;
                }
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                bool isDone = _downloadFileRequest.IsDone;
                if (_enableResume)
                {
                    long downloadedBytes = _fileOriginLength + _downloadFileRequest.DownloadedBytes;
                    float downloadProgress = (float)((double)downloadedBytes / Bundle.FileSize);
                    LatestReport = DownloadReport.CreateProgress(downloadedBytes, downloadProgress);
                    Progress = downloadProgress;
                }
                else
                {
                    LatestReport = DownloadReport.CreateProgress(_downloadFileRequest.DownloadedBytes, _downloadFileRequest.DownloadProgress);
                    Progress = _downloadFileRequest.DownloadProgress;
                }
                if (isDone == false)
                    return;

                // 更新下载报告
                LatestReport = DownloadReport.CreateFinished(_downloadFileRequest.HttpCode, _downloadFileRequest.HttpError,
                    LatestReport.DownloadedBytes, LatestReport.DownloadProgress);

                // 检查网络错误
                if (_downloadFileRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _steps = ESteps.CacheFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadFileRequest.Error);

                    if (_enableResume)
                    {
                        // 注意: HTTP 416 Range Not Satisfiable 表示服务器无法满足客户端在 Range 请求头中指定的字节范围请求。
                        if (_downloadFileRequest.HttpCode == 416)
                            DeleteTempFile();
                    }
                    else
                    {
                        DeleteTempFile();
                    }
                }
            }

            // 缓存文件
            if (_steps == ESteps.CacheFile)
            {
                if (_writeCacheOp == null)
                {
                    var options = new BCWriteCacheOptions(Bundle, _tempFilePath);
                    _writeCacheOp = _fileSystem.BundleCache.WriteCacheAsync(options);
                    _writeCacheOp.StartOperation();
                    AddChildOperation(_writeCacheOp);
                }

                _writeCacheOp.UpdateOperation();
                if (_writeCacheOp.IsDone == false)
                    return;

                if (_writeCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_writeCacheOp.Error);
                }

                // 注意：缓存完成后直接删除临时文件
                DeleteTempFile();
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadFileRequest != null)
            {
                _downloadFileRequest.Dispose();
                _downloadFileRequest = null;
            }
        }
        protected override void InternalWaitForCompletion()
        {
            throw new YooInternalException($"{nameof(DownloadAndCacheFileOperation)} does not support synchronous waiting. Bundle: '{Bundle.BundleName}', Url: '{Url}'.");
        }

        private IDownloadFileRequest CreateResumeRequest()
        {
            // 获取下载起始位置
            if (File.Exists(_tempFilePath))
            {
                FileInfo fileInfo = new FileInfo(_tempFilePath);
                if (fileInfo.Length >= Bundle.FileSize)
                {
                    DeleteTempFile();
                }
                else
                {
                    _fileOriginLength = fileInfo.Length;
                }
            }

            int watchdogTime = _fileSystem.DownloadWatchdogTimeout;
            int timeout = 0; //注意：文件下载不做超时检测
            bool appendToFile = true;
            bool removeFileOnAbort = false;
            long resumeOffset = _fileOriginLength;
            var args = new DownloadFileRequestArgs(
                url: Url,
                timeout: timeout,
                watchdogTimeout: watchdogTime,
                savePath: _tempFilePath,
                appendToFile: appendToFile,
                removeFileOnAbort: removeFileOnAbort,
                resumeOffset: resumeOffset);
            return _fileSystem.DownloadBackend.CreateFileRequest(args);
        }
        private IDownloadFileRequest CreateNormalRequest()
        {
            DeleteTempFile();

            int watchdogTime = _fileSystem.DownloadWatchdogTimeout;
            int timeout = 0; //注意：文件下载不做超时检测
            var args = new DownloadFileRequestArgs(
                url: Url,
                timeout: timeout,
                watchdogTimeout: watchdogTime,
                savePath: _tempFilePath);
            return _fileSystem.DownloadBackend.CreateFileRequest(args);
        }
        private void DeleteTempFile()
        {
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }
    }
}