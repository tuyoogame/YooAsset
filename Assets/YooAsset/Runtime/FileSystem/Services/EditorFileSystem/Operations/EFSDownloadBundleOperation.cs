
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件系统的下载文件操作
    /// </summary>
    internal sealed class EFSDownloadBundleOperation : FSDownloadBundleOperation
    {
        private enum ESteps
        {
            None,
            CheckExists,
            CreateDownload,
            CheckDownload,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private readonly FSDownloadBundleOptions _options;
        private DownloadFileBaseOperation _downloadFileOp;
        private ESteps _steps = ESteps.None;

        internal EFSDownloadBundleOperation(EditorFileSystem fileSystem, FSDownloadBundleOptions options) : base(options.Bundle)
        {
            _fileSystem = fileSystem;
            _options = options;
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
                if (_fileSystem.BundleCache.IsCached(_options.Bundle.BundleGuid))
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
                    string editorFilePath = EditorFileSystemHelper.GetEditorFilePath(Bundle);
                    if (string.IsNullOrEmpty(editorFilePath))
                    {
                        _steps = ESteps.Done;
                        SetError($"Editor file path is empty for bundle '{Bundle.BundleName}'.");
                        return;
                    }

                    _downloadFileOp = new SimulateAndCacheFileOperation(_fileSystem, Bundle, editorFilePath);
                    _fileSystem.DownloadScheduler.RegisterDownloadOperation(_downloadFileOp);
                }

                _steps = ESteps.CheckDownload;
            }

            // 检测结果
            if (_steps == ESteps.CheckDownload)
            {
                if (IsWaitForCompletion)
                {
                    if (_downloadFileOp is SimulateAndCacheFileOperation)
                    {
                        _steps = ESteps.Done;
                        SetError($"Attempting to load bundle '{Bundle.BundleName}' from simulate: '{_downloadFileOp.Url}'.");
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
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadFileOp.Error);
                    YooLogger.LogError(Error);
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
    }
}