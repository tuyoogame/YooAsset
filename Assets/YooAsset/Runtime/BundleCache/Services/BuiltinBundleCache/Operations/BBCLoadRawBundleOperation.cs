
namespace YooAsset
{
    /// <summary>
    /// 内置文件缓存加载 RawBundle 操作
    /// </summary>
    internal sealed class BBCLoadRawBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            GetEntry,
            LoadBundle,
            Done,
        }

        private readonly BuiltinBundleCache _fileCache;
        private readonly PackageBundle _bundle;
        private LoadLocalRawBundleOperation _loadLocalRawBundleOp;
        private BuiltinBundleCacheEntry _cacheEntry;
        private ESteps _steps = ESteps.None;

        public BBCLoadRawBundleOperation(BuiltinBundleCache fileCache, PackageBundle bundle)
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
                if(_loadLocalRawBundleOp == null)
                {
                    var options = new LoadLocalRawBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _bundle,
                        filePath: _cacheEntry.FilePath,
                        rawBundleDecryptor: _fileCache.Config.RawBundleDecryptor);
                    _loadLocalRawBundleOp = new LoadLocalRawBundleOperation(options);
                    _loadLocalRawBundleOp.StartOperation();
                    AddChildOperation(_loadLocalRawBundleOp);
                }

                if (IsWaitForCompletion)
                    _loadLocalRawBundleOp.WaitForCompletion();

                _loadLocalRawBundleOp.UpdateOperation();
                if (_loadLocalRawBundleOp.IsDone == false)
                    return;

                if(_loadLocalRawBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadLocalRawBundleOp.BundleHandle == null)
                        throw new YooInternalException("Loaded raw bundle handle is null.");

                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = _loadLocalRawBundleOp.BundleHandle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadLocalRawBundleOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}