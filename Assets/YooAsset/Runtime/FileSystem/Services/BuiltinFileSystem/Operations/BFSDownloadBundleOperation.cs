
namespace YooAsset
{
    /// <summary>
    /// 内置文件系统的解压文件操作
    /// </summary>
    internal sealed class BFSDownloadBundleOperation : FSDownloadBundleOperation
    {
        private enum ESteps
        {
            None,
            CheckExists,
            CreateUnpack,
            CheckUnpack,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly FSDownloadBundleOptions _options;
        private DownloadFileBaseOperation _downloadFileOp;
        private ESteps _steps = ESteps.None;

        internal BFSDownloadBundleOperation(BuiltinFileSystem fileSystem, FSDownloadBundleOptions options) : base(options.Bundle)
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
                if (_fileSystem.UnpackBundleCache.IsCached(Bundle.BundleGuid))
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.CreateUnpack;
                }
            }

            // 创建解压器
            if (_steps == ESteps.CreateUnpack)
            {
                _downloadFileOp = _fileSystem.UnpackScheduler.TryGetDownloadOperation(Bundle);
                if (_downloadFileOp == null)
                {
                    string builtinFilePath = _fileSystem.GetBuiltinBundleFilePath(Bundle);
                    _downloadFileOp = new UnpackAndCacheFileOperation(_fileSystem, Bundle, builtinFilePath);
                    _fileSystem.UnpackScheduler.RegisterDownloadOperation(_downloadFileOp);
                }

                _steps = ESteps.CheckUnpack;
            }

            // 检测结果
            if (_steps == ESteps.CheckUnpack)
            {
                if (IsWaitForCompletion)
                    _downloadFileOp.WaitForCompletion();

                // 注意：不主动调用 _downloadFileOp.UpdateOperation()
                // 说明：解压任务由 UnpackScheduler 统一驱动，此处仅读取状态。
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
