
namespace YooAsset
{
    /// <summary>
    /// WebGL 平台网络缓存加载 RawBundle 操作
    /// </summary>
    internal sealed class WNBCLoadRawBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBundle,
            Done,
        }

        private readonly WebNetworkBundleCache _fileCache;
        private readonly BCLoadBundleOptions _options;
        private BCLoadBundleOperation _loadBundleOp;
        private ESteps _steps = ESteps.None;

        internal WNBCLoadRawBundleOperation(WebNetworkBundleCache fileCache, BCLoadBundleOptions options)
        {
            _fileCache = fileCache;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadBundle;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBundle)
            {
                if (_loadBundleOp == null)
                {
                    var urls = _fileCache.Config.RemoteService.GetRemoteUrls(_options.Bundle.GetFileName());
                    var options = new LoadWebRawBundleOptions(
                        cacheName: _fileCache.GetType().Name,
                        bundle: _options.Bundle,
                        candidateUrls: urls,
                        rawBundleDecryptor: _fileCache.Config.RawBundleDecryptor,
                        downloadBackend: _fileCache.Config.DownloadBackend,
                        downloadVerifyLevel: _fileCache.Config.DownloadVerifyLevel,
                        watchdogTimeout: 0,
                        downloadRetryPolicy: _fileCache.Config.DownloadRetryPolicy,
                        downloadUrlPolicy: _fileCache.Config.DownloadUrlPolicy);
                    _loadBundleOp = new LoadWebRawBundleOperation(options);
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
