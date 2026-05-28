
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件缓存加载 ArchiveBundle 操作
    /// </summary>
    internal sealed class SBCLoadArchiveBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            GetEntry,
            LoadBundle,
            Done,
        }

        private readonly SandboxBundleCache _fileCache;
        private readonly PackageBundle _bundle;
        private LoadLocalArchiveBundleOperation _loadLocalArchiveBundleOp;
        private SandboxBundleCacheEntry _cacheEntry;

        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建沙盒 ArchiveBundle 加载操作实例
        /// </summary>
        /// <param name="fileCache">沙盒文件缓存系统</param>
        /// <param name="bundle">资源包描述</param>
        public SBCLoadArchiveBundleOperation(SandboxBundleCache fileCache, PackageBundle bundle)
        {
            _fileCache = fileCache;
            _bundle = bundle;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.GetEntry;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetEntry)
            {
                _cacheEntry = _fileCache.GetEntry(_bundle.BundleGuid);
                if (_cacheEntry == null)
                {
                    _steps = ESteps.Done;
                    SetError($"File cache entry not found: '{_bundle.BundleGuid}'.");
                }
                else
                {
                    _steps = ESteps.LoadBundle;
                }
            }

            if (_steps == ESteps.LoadBundle)
            {
                if (_loadLocalArchiveBundleOp == null)
                {
                    var options = new LoadLocalArchiveBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _bundle,
                        filePath: _cacheEntry.DataFilePath,
                        archiveBundleDecryptor: _fileCache.Config.ArchiveBundleDecryptor);
                    _loadLocalArchiveBundleOp = new LoadLocalArchiveBundleOperation(options);
                    _loadLocalArchiveBundleOp.StartOperation();
                    AddChildOperation(_loadLocalArchiveBundleOp);
                }

                if (IsWaitForCompletion)
                    _loadLocalArchiveBundleOp.WaitForCompletion();

                _loadLocalArchiveBundleOp.UpdateOperation();
                if (_loadLocalArchiveBundleOp.IsDone == false)
                    return;

                if (_loadLocalArchiveBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadLocalArchiveBundleOp.BundleHandle == null)
                        throw new YooInternalException("Loaded archive bundle handle is null.");

                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = _loadLocalArchiveBundleOp.BundleHandle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadLocalArchiveBundleOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}