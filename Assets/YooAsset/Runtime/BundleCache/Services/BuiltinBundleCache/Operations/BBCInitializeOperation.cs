
namespace YooAsset
{
    /// <summary>
    /// 内置文件缓存初始化操作
    /// </summary>
    internal sealed class BBCInitializeOperation : BCInitializeOperation
    {
        private enum ESteps
        {
            None,
            LoadCatalog,
            RecordEntry,
            Done,
        }

        private readonly BuiltinBundleCache _fileCache;
        private LoadBuiltinCatalogOperation _loadBuiltinCatalogOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建内置缓存初始化操作实例
        /// </summary>
        /// <param name="fileCache">内置文件缓存系统</param>
        public BBCInitializeOperation(BuiltinBundleCache fileCache)
        {
            _fileCache = fileCache;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadCatalog;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadCatalog)
            {
                if (_loadBuiltinCatalogOp == null)
                {
                    var options = new LoadBuiltinCatalogOptions(
                        packageName: _fileCache.PackageName,
                        filePath: _fileCache.GetCatalogBinaryFileLoadPath(),
                        downloadBackend: _fileCache.Config.DownloadBackend);
                    _loadBuiltinCatalogOp = new LoadBuiltinCatalogOperation(options);
                    _loadBuiltinCatalogOp.StartOperation();
                    AddChildOperation(_loadBuiltinCatalogOp);
                }

                _loadBuiltinCatalogOp.UpdateOperation();
                if (_loadBuiltinCatalogOp.IsDone == false)
                    return;

                if (_loadBuiltinCatalogOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.RecordEntry;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadBuiltinCatalogOp.Error);
                }
            }

            if (_steps == ESteps.RecordEntry)
            {
                var catalog = _loadBuiltinCatalogOp.Catalog;
                foreach (var fileEntry in catalog.Entries)
                {
                    string filePath = PathUtility.Combine(_fileCache.RootPath, fileEntry.FileName);
                    var cacheEntry = new BuiltinBundleCacheEntry(fileEntry.BundleGuid, filePath);
                    _fileCache.AddEntry(fileEntry.BundleGuid, cacheEntry);
                }

                _steps = ESteps.Done;
                SetResult();
            }
        }
    }
}
