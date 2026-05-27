
namespace YooAsset
{
    /// <summary>
    /// 内置文件系统的加载资源包操作
    /// </summary>
    internal sealed class BFSLoadPackageBundleOperation : FSLoadPackageBundleOperation
    {
        private enum ESteps
        {
            None,
            Prepare,
            UnpackFile,
            AbortUnpack,
            LoadUnpackBundle,
            LoadBundle,
            CheckResult,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly FSLoadPackageBundleOptions _options;
        private FSDownloadBundleOperation _unpackFileOp;
        private BCLoadBundleOperation _loadBundleOp;
        private ESteps _steps = ESteps.None;

        internal BFSLoadPackageBundleOperation(BuiltinFileSystem fileSystem, FSLoadPackageBundleOptions options)
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
                if (_fileSystem.IsUnpackBundle(_options.Bundle))
                {
                    if (_fileSystem.UnpackBundleCache.IsCached(_options.Bundle.BundleGuid))
                        _steps = ESteps.LoadUnpackBundle;
                    else
                        _steps = ESteps.UnpackFile;
                }
                else
                {
                    _steps = ESteps.LoadBundle;
                }
            }

            if (_steps == ESteps.UnpackFile)
            {
                // 中断解压
                if (ShouldAbortDownload)
                {
                    if (_unpackFileOp != null)
                        _unpackFileOp.AbortOperation();
                    _steps = ESteps.AbortUnpack;
                }
            }

            if (_steps == ESteps.UnpackFile)
            {
                if (_unpackFileOp == null)
                {
                    // 注意：内置文件解压不做失败尝试
                    var options = new FSDownloadBundleOptions(_options.Bundle, 0);
                    _unpackFileOp = _fileSystem.DownloadBundleAsync(options);
                    _unpackFileOp.StartOperation();
                    AddChildOperation(_unpackFileOp);
                }

                if (IsWaitForCompletion)
                    _unpackFileOp.WaitForCompletion();

                _unpackFileOp.UpdateOperation();
                if (_unpackFileOp.IsDone == false)
                    return;

                if (_unpackFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadUnpackBundle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_unpackFileOp.Error);
                }
            }

            if (_steps == ESteps.AbortUnpack)
            {
                if (_unpackFileOp != null)
                {
                    if (IsWaitForCompletion)
                        _unpackFileOp.WaitForCompletion();

                    _unpackFileOp.UpdateOperation();
                    if (_unpackFileOp.IsDone == false)
                        return;
                }

                _steps = ESteps.Done;
                SetError("Bundle download aborted.");
            }

            if (_steps == ESteps.LoadUnpackBundle)
            {
                _loadBundleOp = _fileSystem.UnpackBundleCache.LoadBundleAsync(_options.ConvertTo());
                _loadBundleOp.StartOperation();
                AddChildOperation(_loadBundleOp);
                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.LoadBundle)
            {
                _loadBundleOp = _fileSystem.BuiltinBundleCache.LoadBundleAsync(_options.ConvertTo());
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