using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 清理编辑器文件缓存操作
    /// </summary>
    internal sealed class EBCClearCacheOperation : BCClearCacheOperation
    {
        private enum ESteps
        {
            None,
            GetResult,
            ClearCacheFiles,
            Done,
        }

        private readonly EditorBundleCache _fileCache;
        private readonly BCClearCacheOptions _options;
        private readonly ICacheEvictionPolicy _policy;
        private IReadOnlyList<string> _bundleGuids;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建编辑器清理缓存操作实例
        /// </summary>
        /// <param name="fileCache">编辑器文件缓存系统</param>
        /// <param name="options">清理缓存选项</param>
        /// <param name="policy">缓存逐出策略</param>
        internal EBCClearCacheOperation(EditorBundleCache fileCache, BCClearCacheOptions options, ICacheEvictionPolicy policy)
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

                _bundleGuids = clearResult.BundleGuids;
                _steps = ESteps.ClearCacheFiles;
            }

            if (_steps == ESteps.ClearCacheFiles)
            {
                foreach (var bundleGuid in _bundleGuids)
                {
                    _fileCache.RemoveEntry(bundleGuid);
                }

                _steps = ESteps.Done;
                SetResult();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
