using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存清理操作
    /// </summary>
    internal sealed class SBCClearCacheOperation : BCClearCacheOperation
    {
        private enum ESteps
        {
            None,
            GetResult,
            ClearCacheFiles,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly BCClearCacheOptions _options;
        private readonly ICacheEvictionPolicy _policy;
        private ClearCacheFilesOperation _clearCacheFilesOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒清理缓存操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="options">清理缓存选项</param>
        /// <param name="policy">缓存淘汰策略</param>
        internal SBCClearCacheOperation(SandboxBundleCache fileCache, BCClearCacheOptions options, ICacheEvictionPolicy policy)
        {
            _fileCache = fileCache;
            _options = options;
            _policy = policy;
        }

        protected override void InternalStart()
        {
            _steps = ESteps.GetResult;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetResult)
            {
                var cacheEntries = _fileCache.GetAllEntries();
                EvictionResult clearResult = _policy.SelectEvictionTargets(cacheEntries, _options);

                if (clearResult.Succeeded == false)
                {
                    _steps = ESteps.Done;
                    SetError(clearResult.Error);
                    return;
                }

                _clearCacheFilesOp = new ClearCacheFilesOperation(_fileCache, new List<string>(clearResult.BundleGuids));
                _clearCacheFilesOp.StartOperation();
                AddChildOperation(_clearCacheFilesOp);
                _steps = ESteps.ClearCacheFiles;
            }

            if (_steps == ESteps.ClearCacheFiles)
            {
                if (IsWaitForCompletion)
                    _clearCacheFilesOp.WaitForCompletion();

                _clearCacheFilesOp.UpdateOperation();
                Progress = _clearCacheFilesOp.Progress;
                if (_clearCacheFilesOp.IsDone == false)
                    return;

                if (_clearCacheFilesOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_clearCacheFilesOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
