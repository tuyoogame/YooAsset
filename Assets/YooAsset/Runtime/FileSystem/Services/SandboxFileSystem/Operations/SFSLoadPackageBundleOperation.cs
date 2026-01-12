
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件系统的加载资源包操作
    /// </summary>
    internal sealed class SFSLoadPackageBundleOperation : FSLoadPackageBundleOperation
    {
        private enum ESteps
        {
            None,
            Prepare,
            DownloadFile,
            AbortDownload,
            LoadBundle,
            CheckResult,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly FSLoadPackageBundleOptions _options;
        private FSDownloadBundleOperation _downloadFileOp;
        private BCLoadBundleOperation _loadBundleOp;
        private ESteps _steps = ESteps.None;

        internal SFSLoadPackageBundleOperation(SandboxFileSystem fileSystem, FSLoadPackageBundleOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.Prepare;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Prepare)
            {
                if (_fileSystem.BundleCache.IsCached(_options.Bundle.BundleGuid))
                {
                    _steps = ESteps.LoadBundle;
                }
                else
                {
                    if (_fileSystem.DisableOnDemandDownload)
                    {
                        _steps = ESteps.Done;
                        SetError($"Bundle is not cached: '{_options.Bundle.BundleName}'.");
                        YooLogger.LogWarning(Error);
                    }
                    else
                    {
                        _steps = ESteps.DownloadFile;
                    }
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                // 中断下载
                if (ShouldAbortDownload)
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
                    var options = new FSDownloadBundleOptions(_options.Bundle, int.MaxValue);
                    _downloadFileOp = _fileSystem.DownloadBundleAsync(options);
                    _downloadFileOp.StartOperation();
                    AddChildOperation(_downloadFileOp);
                }

                if (IsWaitForCompletion)
                    _downloadFileOp.WaitForCompletion();

                _downloadFileOp.UpdateOperation();
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadBundle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadFileOp.Error);
                }
            }

            if (_steps == ESteps.AbortDownload)
            {
                if (_downloadFileOp != null)
                {
                    if (IsWaitForCompletion)
                        _downloadFileOp.WaitForCompletion();

                    _downloadFileOp.UpdateOperation();
                    if (_downloadFileOp.IsDone == false)
                        return;
                }

                _steps = ESteps.Done;
                SetError("Bundle download aborted.");
            }

            if (_steps == ESteps.LoadBundle)
            {
                _loadBundleOp = _fileSystem.BundleCache.LoadBundleAsync(_options.ConvertTo());
                _loadBundleOp.StartOperation();
                AddChildOperation(_loadBundleOp);
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (IsWaitForCompletion)
                    _loadBundleOp.WaitForCompletion();

                _loadBundleOp.UpdateOperation();
                if (_loadBundleOp.IsDone == false)
                    return;

                if (_loadBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadBundleOp.BundleHandle == null)
                    {
                        _steps = ESteps.Done;
                        SetError("Fatal error: loaded bundle handle is null.");
                        YooLogger.LogError(Error);
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetResult();
                        BundleHandle = _loadBundleOp.BundleHandle;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadBundleOp.Error);
                    YooLogger.LogError(Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}