
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存验证操作
    /// </summary>
    internal sealed class SBCVerifyCacheOperation : BCVerifyCacheOperation
    {
        private enum ESteps
        {
            None,
            Check,
            VerifyFile,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly BCVerifyCacheOptions _options;
        private VerifyTempFileOperation _verifyTempFileOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒验证缓存操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="options">验证缓存选项</param>
        public SBCVerifyCacheOperation(SandboxBundleCache fileCache, BCVerifyCacheOptions options)
        {
            _fileCache = fileCache;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.Check;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Check)
            {
                if (_fileCache.IsCached(_options.Bundle.BundleGuid) == false)
                {
                    _steps = ESteps.Done;
                    SetError($"File cache entry not found: '{_options.Bundle.BundleGuid}'.");
                }
                else
                {
                    _steps = ESteps.VerifyFile;
                }
            }

            if (_steps == ESteps.VerifyFile)
            {
                if (_verifyTempFileOp == null)
                {
                    var entry = _fileCache.GetEntry(_options.Bundle.BundleGuid);
                    if (entry == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"File cache entry not found: '{_options.Bundle.BundleGuid}'.");
                        return;
                    }

                    var element = new TempFileInfo(entry.DataFilePath, _options.Bundle.FileCrc, _options.Bundle.FileSize);
                    _verifyTempFileOp = new VerifyTempFileOperation(element);
                    _verifyTempFileOp.StartOperation();
                    AddChildOperation(_verifyTempFileOp);
                }

                if (IsWaitForCompletion)
                    _verifyTempFileOp.WaitForCompletion();

                _verifyTempFileOp.UpdateOperation();
                if (_verifyTempFileOp.IsDone == false)
                    return;

                if (_verifyTempFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_verifyTempFileOp.Error);

                    if (_options.DeleteCacheEntryOnFailure)
                    {
                        YooLogger.LogError($"Found corrupted bundle file. Removing cache entry: '{_options.Bundle.BundleGuid}'.");
                        _fileCache.RemoveEntry(_options.Bundle.BundleGuid);
                    }
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
