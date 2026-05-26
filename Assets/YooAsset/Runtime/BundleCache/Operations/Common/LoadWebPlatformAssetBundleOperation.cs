namespace YooAsset
{
    /// <summary>
    /// WebGL 平台加载非加密 AssetBundle 操作
    /// </summary>
    internal sealed class LoadWebPlatformAssetBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            BundleRequest,
            CheckRequest,
            TryAgain,
            Done,
        }

        private readonly LoadWebPlatformAssetBundleOptions _options;
        private readonly DownloadRetryController _downloadRetryController;
        private IDownloadAssetBundleRequest _downloadAssetBundleRequest;
        private ESteps _steps = ESteps.None;

        internal LoadWebPlatformAssetBundleOperation(LoadWebPlatformAssetBundleOptions options)
        {
            _options = options;

            // 注意：网络原因失败后，重新尝试直到成功
            _downloadRetryController = new DownloadRetryController(int.MaxValue, options.DownloadRetryPolicy);
        }
        protected override void InternalStart()
        {
            _steps = ESteps.BundleRequest;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.BundleRequest)
            {
                string url = _options.DownloadUrlPolicy.SelectUrl(_options.CandidateUrls);
                var args = new DownloadAssetBundleRequestArgs(
                    url: url,
                    timeout: 0,
                    watchdogTimeout: _options.WatchdogTimeout,
                    disableUnityWebCache: _options.DisableUnityWebCache,
                    fileHash: _options.Bundle.FileHash,
                    unityCrc: _options.Bundle.UnityCrc,
                    platformStrategy: _options.PlatformStrategy);
                _downloadAssetBundleRequest = _options.DownloadBackend.CreateAssetBundleRequest(args);
                _downloadAssetBundleRequest.SendRequest();
                _steps = ESteps.CheckRequest;
            }

            if (_steps == ESteps.CheckRequest)
            {
                //TODO 部分小游戏平台的 downloadProgress 始终返回 0，导致进度条无法正确显示。
                Progress = _downloadAssetBundleRequest.DownloadProgress;
                if (_downloadAssetBundleRequest.IsDone == false)
                    return;

                if (_downloadAssetBundleRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _options.DownloadUrlPolicy.OnRequestSucceeded(_downloadAssetBundleRequest.Url);
                    var assetBundle = _downloadAssetBundleRequest.Result;
                    if (assetBundle == null)
                    {
                        _steps = ESteps.Done;
                        SetError($"Downloaded asset bundle is null. URL: {_downloadAssetBundleRequest.Url}");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetResult();
                        BundleHandle = new WebAssetBundleHandle(_options.Bundle, assetBundle, _options.PlatformStrategy);
                    }
                }
                else
                {
                    string url = _downloadAssetBundleRequest.Url;
                    long httpCode = _downloadAssetBundleRequest.HttpCode;
                    string httpError = _downloadAssetBundleRequest.HttpError;
                    _options.DownloadUrlPolicy.OnRequestFailed(url, httpCode, httpError);

                    if (IsWaitForCompletion == false && _downloadRetryController.CanRetryRequest(url, httpCode, httpError))
                    {
                        _downloadRetryController.StartRetryDelay();
                        _steps = ESteps.TryAgain;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError(_downloadAssetBundleRequest.Error);
                        YooLogger.LogError(Error);
                    }
                }
            }

            if (_steps == ESteps.TryAgain)
            {
                // 注意：失败后释放网络请求
                if (_downloadAssetBundleRequest != null)
                {
                    _downloadAssetBundleRequest.Dispose();
                    _downloadAssetBundleRequest = null;
                }

                if (_downloadRetryController.TickRetryDelay())
                {
                    Progress = 0f;
                    _steps = ESteps.BundleRequest;
                }
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadAssetBundleRequest != null)
            {
                _downloadAssetBundleRequest.Dispose();
                _downloadAssetBundleRequest = null;
            }
        }
    }
}
