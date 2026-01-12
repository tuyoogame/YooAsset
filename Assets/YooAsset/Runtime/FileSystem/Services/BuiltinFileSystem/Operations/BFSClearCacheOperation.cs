
namespace YooAsset
{
    /// <summary>
    /// 内置文件系统的清理缓存操作
    /// </summary>
    internal sealed class BFSClearCacheOperation : FSClearCacheOperation
    {
        private enum ESteps
        {
            None,
            CheckReadOnly,
            ClearCache,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly FSClearCacheOptions _options;
        private BCClearCacheOperation _clearCacheOp;
        private ESteps _steps = ESteps.None;

        internal BFSClearCacheOperation(BuiltinFileSystem fileSystem, FSClearCacheOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckReadOnly;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckReadOnly)
            {
                if (_fileSystem.UnpackBundleCache.IsReadOnly)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    return;
                }

                _steps = ESteps.ClearCache;
            }

            if (_steps == ESteps.ClearCache)
            {
                if (_clearCacheOp == null)
                {
                    _clearCacheOp = _fileSystem.UnpackBundleCache.ClearCacheAsync(_options.ConvertTo());
                    _clearCacheOp.StartOperation();
                    AddChildOperation(_clearCacheOp);
                }

                _clearCacheOp.UpdateOperation();
                if (_clearCacheOp.IsDone == false)
                    return;

                if (_clearCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_clearCacheOp.Error);
                }
            }
        }
    }
}