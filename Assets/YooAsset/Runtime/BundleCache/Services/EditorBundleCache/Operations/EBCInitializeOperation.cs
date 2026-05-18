namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存初始化操作
    /// </summary>
    internal sealed class EBCInitializeOperation : BCInitializeOperation
    {
        private enum ESteps
        {
            None,
            ScanMarkerFiles,
            AddCacheEntries,
            Done,
        }

        private readonly EditorBundleCache _fileCache;
        private ScanMarkerFilesOperation _scanMarkerFilesOp;
        private ESteps _steps = ESteps.None;

        public EBCInitializeOperation(EditorBundleCache cache)
        {
            _fileCache = cache;
        }
        protected override void InternalStart()
        {
            if (_fileCache.Config.VirtualDownloadMode == false)
            {
                SetResult();
                return;
            }

            _steps = ESteps.ScanMarkerFiles;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ScanMarkerFiles)
            {
                if (_scanMarkerFilesOp == null)
                {
                    _scanMarkerFilesOp = new ScanMarkerFilesOperation(_fileCache);
                    _scanMarkerFilesOp.StartOperation();
                    AddChildOperation(_scanMarkerFilesOp);
                }

                _scanMarkerFilesOp.UpdateOperation();
                Progress = _scanMarkerFilesOp.Progress;
                if (_scanMarkerFilesOp.IsDone == false)
                    return;

                if (_scanMarkerFilesOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.AddCacheEntries;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_scanMarkerFilesOp.Error);
                }
            }

            if (_steps == ESteps.AddCacheEntries)
            {
                foreach (var fileInfo in _scanMarkerFilesOp.Result)
                {
                    if (_fileCache.IsCached(fileInfo.BundleGuid) == false)
                    {
                        var cacheEntry = new EditorBundleCacheEntry(fileInfo.BundleGuid, fileInfo.MarkerFilePath);
                        _fileCache.AddEntry(fileInfo.BundleGuid, cacheEntry);
                    }
                }

                _steps = ESteps.Done;
                SetResult();
            }
        }
    }
}
