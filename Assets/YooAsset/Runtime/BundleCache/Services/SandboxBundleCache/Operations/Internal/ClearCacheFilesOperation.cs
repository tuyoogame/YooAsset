using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 清理缓存文件操作
    /// </summary>
    internal sealed class ClearCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckParam,
            ClearCache,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly List<string> _bundleGuids;
        private int _fileTotalCount;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建清理缓存文件操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="bundleGuids">要清理的资源包 GUID 列表</param>
        public ClearCacheFilesOperation(SandboxBundleCache fileCache, List<string> bundleGuids)
        {
            _fileCache = fileCache;
            _bundleGuids = bundleGuids;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckParam;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParam)
            {
                if (_bundleGuids == null || _bundleGuids.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    return;
                }

                _fileTotalCount = _bundleGuids.Count;
                _steps = ESteps.ClearCache;
            }

            if (_steps == ESteps.ClearCache)
            {
                for (int i = _bundleGuids.Count - 1; i >= 0; i--)
                {
                    string bundleGuid = _bundleGuids[i];
                    _fileCache.RemoveEntry(bundleGuid);
                    _bundleGuids.RemoveAt(i);
                    if (IsBusy)
                        break;
                }

                if (_fileTotalCount == 0)
                    Progress = 1.0f;
                else
                    Progress = 1.0f - ((float)_bundleGuids.Count / _fileTotalCount);

                if (_bundleGuids.Count == 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
