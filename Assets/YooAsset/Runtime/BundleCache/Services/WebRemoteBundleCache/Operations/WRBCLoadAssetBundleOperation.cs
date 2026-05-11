
namespace YooAsset
{
    /// <summary>
    /// Web远端文件缓存加载 AssetBundle 操作
    /// </summary>
    internal sealed class WRBCLoadAssetBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            GetEntry,
            LoadBundle,
            Done,
        }

        private readonly WebRemoteBundleCache _fileCache;
        private readonly BCLoadBundleOptions _options;
        private BCLoadBundleOperation _loadBundleOp;
        private WebRemoteBundleCacheEntry _cacheEntry;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建 WRBCLoadAssetBundleOperation 实例
        /// </summary>
        /// <param name="fileCache">Web 远端文件缓存系统</param>
        /// <param name="options">加载资源包操作选项</param>
        public WRBCLoadAssetBundleOperation(WebRemoteBundleCache fileCache, BCLoadBundleOptions options)
        {
            _fileCache = fileCache;
            _options = options;
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
                _cacheEntry = _fileCache.GetEntry(_options.Bundle);
                if (_cacheEntry == null)
                {
                    _steps = ESteps.Done;
                    SetError($"File cache entry not found: '{_options.Bundle.BundleGuid}'.");
                }
                else
                {
                    _steps = ESteps.LoadBundle;
                }
            }

            if (_steps == ESteps.LoadBundle)
            {
                if (_loadBundleOp == null)
                {
                    var options = new LoadWebAssetBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _options.Bundle,
                        candidateUrls: _cacheEntry.Urls,
                        assetBundleDecryptor: _fileCache.Config.AssetBundleDecryptor,
                        downloadBackend: _fileCache.Config.DownloadBackend,
                        downloadVerifyLevel: _fileCache.Config.DownloadVerifyLevel,
                        watchdogTimeout: _fileCache.Config.WatchdogTimeout,
                        disableUnityWebCache: _fileCache.Config.DisableUnityWebCache,
                        downloadRetryPolicy: _fileCache.Config.DownloadRetryPolicy,
                        downloadUrlPolicy: _fileCache.Config.DownloadUrlPolicy);

                    if (_options.Bundle.IsEncrypted)
                        _loadBundleOp = new LoadWebEncryptedAssetBundleOperation(options);
                    else
                        _loadBundleOp = new LoadWebNormalAssetBundleOperation(options);

                    _loadBundleOp.StartOperation();
                    AddChildOperation(_loadBundleOp);
                }

                _loadBundleOp.UpdateOperation();
                Progress = _loadBundleOp.Progress;
                if (_loadBundleOp.IsDone == false)
                    return;

                if (_loadBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadBundleOp.BundleHandle == null)
                        throw new YooInternalException("Loaded bundle handle is null.");

                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = _loadBundleOp.BundleHandle;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadBundleOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            if (_steps != ESteps.Done)
            {
                _steps = ESteps.Done;
                SetError("WebGL platform does not support synchronous loading.");
                YooLogger.LogError(Error);
            }
        }
    }
}