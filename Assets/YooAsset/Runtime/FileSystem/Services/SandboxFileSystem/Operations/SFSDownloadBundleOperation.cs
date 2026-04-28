using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件系统的下载文件操作
    /// </summary>
    internal sealed class SFSDownloadBundleOperation : FSDownloadBundleOperation
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

        private readonly SandboxFileSystem _fileSystem;
        private readonly FSDownloadBundleOptions _options;
        private readonly DownloadRetryController _downloadRetryController;
        private IReadOnlyList<string> _candidateUrls;
        private DownloadFileBaseOperation _downloadFileOp;
        private ESteps _steps = ESteps.None;

        internal SFSDownloadBundleOperation(SandboxFileSystem fileSystem, FSDownloadBundleOptions options) : base(options.Bundle)
        {
            _fileSystem = fileSystem;
            _options = options;
            _downloadRetryController = new DownloadRetryController(options.RetryCount, _fileSystem.DownloadRetryPolicy);
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
                if (_fileSystem.BundleCache.IsCached(Bundle.BundleGuid))
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
                    if (string.IsNullOrEmpty(_options.ImportFilePath))
                    {
                        // 下载远端文件
                        string url = GetRequestUrl(Bundle.GetFileName());
                        _downloadFileOp = new DownloadAndCacheFileOperation(_fileSystem, Bundle, url);
                        _fileSystem.DownloadScheduler.RegisterDownloadOperation(_downloadFileOp);
                    }
                    else
                    {
                        // 导入本地文件
                        _downloadFileOp = new ImportAndCacheFileOperation(_fileSystem, Bundle, _options.ImportFilePath);
                        _fileSystem.DownloadScheduler.RegisterDownloadOperation(_downloadFileOp);
                    }
                }

                _steps = ESteps.CheckDownload;
            }

            // 检测结果
            if (_steps == ESteps.CheckDownload)
            {
                if (IsWaitForCompletion)
                {
                    if (_downloadFileOp is DownloadAndCacheFileOperation)
                    {
                        _steps = ESteps.Done;
                        SetError($"Attempting to load bundle '{Bundle.BundleName}' from remote: '{_downloadFileOp.Url}'.");
                        return;
                    }
                    _downloadFileOp.WaitForCompletion();
                }

                // 注意：不主动调用 _downloadFileOp.UpdateOperation()
                // 说明：下载任务由 DownloadSchedulerOperation 统一驱动，此处仅读取状态。
                // 说明：同步等待由 WaitForCompletion() 内部的 ExecuteBatch() 保证。
                Progress = _downloadFileOp.Progress;
                Report = _downloadFileOp.LatestReport;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeeded)
                {
                    _fileSystem.DownloadUrlPolicy.OnRequestSucceeded(_downloadFileOp.Url);
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    // 注意：本地导入失败时跳过重试策略
                    if (string.IsNullOrEmpty(_options.ImportFilePath) == false)
                    {
                        _steps = ESteps.Done;
                        SetError(_downloadFileOp.Error);
                        YooLogger.LogError(Error);
                    }
                    else
                    {
                        string url = _downloadFileOp.Url;
                        long httpCode = _downloadFileOp.LatestReport.HttpCode;
                        string httpError = _downloadFileOp.LatestReport.HttpError;
                        _fileSystem.DownloadUrlPolicy.OnRequestFailed(url, httpCode, httpError);
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
            }

            // 重新尝试下载
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
        protected override void InternalAbort()
        {
            // 注意：取消下载任务的时候引用计数减一
            if (_steps != ESteps.Done)
            {
                if (_downloadFileOp != null)
                {
                    _downloadFileOp.Release();
                }
            }
        }

        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        private string GetRequestUrl(string fileName)
        {
            if (_candidateUrls == null)
                _candidateUrls = _fileSystem.RemoteService.GetRemoteUrls(fileName);
            
            return _fileSystem.DownloadUrlPolicy.SelectUrl(_candidateUrls);
        }
    }
}