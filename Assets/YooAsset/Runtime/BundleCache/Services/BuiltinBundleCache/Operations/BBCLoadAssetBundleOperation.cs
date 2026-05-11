
namespace YooAsset
{
    /// <summary>
    /// 内置文件缓存加载 AssetBundle 操作
    /// </summary>
    internal sealed class BBCLoadAssetBundleOperation : BCLoadBundleOperation
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
        private LoadLocalAssetBundleOperation _loadLocalAssetBundleOp;
        private BuiltinBundleCacheEntry _cacheEntry;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建内置 AssetBundle 加载操作实例
        /// </summary>
        /// <param name="fileCache">内置文件缓存系统</param>
        /// <param name="bundle">资源包描述</param>
        public BBCLoadAssetBundleOperation(BuiltinBundleCache fileCache, PackageBundle bundle)
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
                if (_loadLocalAssetBundleOp == null)
                {
                    var options = new LoadLocalAssetBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _bundle,
                        filePath: _cacheEntry.FilePath,
                        assetBundleDecryptor: _fileCache.Config.AssetBundleDecryptor);
                    _loadLocalAssetBundleOp = new LoadLocalAssetBundleOperation(options);
                    _loadLocalAssetBundleOp.StartOperation();
                    AddChildOperation(_loadLocalAssetBundleOp);
                }

                if (IsWaitForCompletion)
                    _loadLocalAssetBundleOp.WaitForCompletion();

                _loadLocalAssetBundleOp.UpdateOperation();
                if (_loadLocalAssetBundleOp.IsDone == false)
                    return;

                if (_loadLocalAssetBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadLocalAssetBundleOp.BundleHandle == null)
                        throw new YooInternalException("Loaded bundle handle is null.");

                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = _loadLocalAssetBundleOp.BundleHandle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadLocalAssetBundleOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}