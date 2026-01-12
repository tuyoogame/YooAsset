
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存初始化操作
    /// </summary>
    internal sealed class SBCInitializeOperation : BCInitializeOperation
    {
        private enum ESteps
        {
            None,
            SearchCacheFiles,
            VerifyCacheFiles,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private SearchCacheFilesOperation _searchCacheFilesOp;
        private VerifyCacheFilesOperation _verifyCacheFilesOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒缓存初始化操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        public SBCInitializeOperation(SandboxBundleCache fileCache)
        {
            _fileCache = fileCache;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.SearchCacheFiles;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.SearchCacheFiles)
            {
                if (_searchCacheFilesOp == null)
                {
                    _searchCacheFilesOp = new SearchCacheFilesOperation(_fileCache);
                    _searchCacheFilesOp.StartOperation();
                    AddChildOperation(_searchCacheFilesOp);
                }

                _searchCacheFilesOp.UpdateOperation();
                Progress = _searchCacheFilesOp.Progress;
                if (_searchCacheFilesOp.IsDone == false)
                    return;

                if (_searchCacheFilesOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.VerifyCacheFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_searchCacheFilesOp.Error);
                }
            }

            if (_steps == ESteps.VerifyCacheFiles)
            {
                if (_verifyCacheFilesOp == null)
                {
                    _verifyCacheFilesOp = new VerifyCacheFilesOperation(_fileCache, _fileCache.Config.FileVerifyLevel, _fileCache.Config.FileVerifyMaxConcurrency, _searchCacheFilesOp.Result);
                    _verifyCacheFilesOp.StartOperation();
                    AddChildOperation(_verifyCacheFilesOp);
                }

                _verifyCacheFilesOp.UpdateOperation();
                Progress = _verifyCacheFilesOp.Progress;
                if (_verifyCacheFilesOp.IsDone == false)
                    return;

                if (_verifyCacheFilesOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_verifyCacheFilesOp.Error);
                }
            }
        }
    }
}
