using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统的资源包下载操作
    /// </summary>
    internal sealed class WNFSDownloadBundleOperation : FSDownloadBundleOperation
    {
        private enum ESteps
        {
            None,
            CheckExists,
            CreateDownload,
            CheckDownload,
            TryAgain,
            Done,
        }

        private readonly WebNetworkFileSystem _fileSystem;
        private readonly DownloadRetryController _downloadRetryController;
        private IReadOnlyList<string> _candidateUrls;
        private DownloadFileBaseOperation _downloadFileOp;
        private ESteps _steps = ESteps.None;

        internal WNFSDownloadBundleOperation(WebNetworkFileSystem fileSystem, FSDownloadBundleOptions options) : base(options.Bundle)
        {
            _fileSystem = fileSystem;
            _downloadRetryController = new DownloadRetryController(options.RetryCount, fileSystem.DownloadRetryPolicy);
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckExists;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 检测文件是否存在
            if (_steps == ESteps.CheckExists)
            {
                if (_fileSystem.IsBundleCached(Bundle))
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.CreateDownload;
                }
            }

            // 创建下载器
            if (_steps == ESteps.CreateDownload)
            {
                _downloadFileOp = _fileSystem.DownloadScheduler.TryGetDownloadOperation(Bundle);
                if (_downloadFileOp == null)
                {
                    string url = GetRequestUrl(Bundle.GetFileName());
                    _downloadFileOp = new PreloadWebFileOperation(_fileSystem, Bundle, url);
                    _fileSystem.DownloadScheduler.RegisterDownloadOperation(_downloadFileOp);
                }

                _steps = ESteps.CheckDownload;
            }

            if (_steps == ESteps.CheckDownload)
            {
                if (IsWaitForCompletion)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(WebNetworkFileSystem)} does not support synchronous download.");
                    return;
                }

                // 注意：不主动调用 _downloadFileOp.UpdateOperation()
                // 注意：下载任务由 DownloadSchedulerOperation 统一驱动，此处仅读取状态。
                Progress = _downloadFileOp.Progress;
                Report = _downloadFileOp.LatestReport;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    string url = _downloadFileOp.Url;
                    long httpCode = _downloadFileOp.LatestReport.HttpCode;
                    string httpError = _downloadFileOp.LatestReport.HttpError;
                    if (IsWaitForCompletion == false && _downloadRetryController.CanRetryRequest(url, httpCode, httpError))
                    {
                        _downloadRetryController.StartRetryDelay();
                        _steps = ESteps.TryAgain;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError(_downloadFileOp.Error);
                        YooLogger.LogError(Error);
                    }
                }
            }

            if (_steps == ESteps.TryAgain)
            {
                if (_downloadRetryController.TickRetryDelay())
                {
                    if (_downloadFileOp != null)
                    {
                        _downloadFileOp.Release();
                        _downloadFileOp = null;
                    }

                    Progress = 0f;
                    Report = DownloadReport.Empty;
                    _steps = ESteps.CreateDownload;
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
        protected override void InternalDispose()
        {
            if (_downloadFileOp != null)
            {
                _downloadFileOp.Release();
                _downloadFileOp = null;
            }
        }

        private string GetRequestUrl(string fileName)
        {
            if (_candidateUrls == null)
                _candidateUrls = _fileSystem.RemoteService.GetRemoteUrls(fileName);

            return _fileSystem.DownloadUrlPolicy.SelectUrl(_candidateUrls);
        }
    }
}
