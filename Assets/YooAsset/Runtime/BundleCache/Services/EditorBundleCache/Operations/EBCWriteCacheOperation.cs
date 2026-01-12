
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存写入操作
    /// </summary>
    internal sealed class EBCWriteCacheOperation : BCWriteCacheOperation
    {
        private enum ESteps
        {
            None,
            CheckCache,
            CacheFile,
            Done,
        }

        private readonly EditorBundleCache _fileCache;
        private readonly BCWriteCacheOptions _options;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建编辑器写入缓存操作实例
        /// </summary>
        /// <param name="fileCache">编辑器文件缓存系统</param>
        /// <param name="options">写入缓存选项</param>
        public EBCWriteCacheOperation(EditorBundleCache fileCache, BCWriteCacheOptions options)
        {
            _fileCache = fileCache;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckCache;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckCache)
            {
                if (_fileCache.IsCached(_options.Bundle.BundleGuid))
                {
                    _steps = ESteps.Done;
                    SetError("Bundle is already cached.");
                }
                else
                {
                    _steps = ESteps.CacheFile;
                }
            }

            if (_steps == ESteps.CacheFile)
            {
                var cacheEntry = new EditorBundleCacheEntry(_options.Bundle.BundleGuid, _options.FilePath);
                _fileCache.AddEntry(_options.Bundle.BundleGuid, cacheEntry);
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
