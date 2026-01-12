using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 从网络加载未加密 AssetBundle 操作
    /// </summary>
    internal sealed class LoadWebNormalAssetBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            BundleRequest,
            CheckRequest,
            TryAgain,
            Done,
        }

        private readonly LoadWebAssetBundleOptions _options;
        private readonly DownloadRetryController _downloadRetryController;
        private IDownloadAssetBundleRequest _downloadAssetBundleRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建 LoadWebNormalAssetBundleOperation 实例
        /// </summary>
        /// <param name="options">从网络加载 AssetBundle 的配置选项</param>
        public LoadWebNormalAssetBundleOperation(LoadWebAssetBundleOptions options)
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
                    unityCrc: _options.Bundle.UnityCrc);
                _downloadAssetBundleRequest = _options.DownloadBackend.CreateAssetBundleRequest(args);
                _downloadAssetBundleRequest.SendRequest();
                _steps = ESteps.CheckRequest;
            }

            if (_steps == ESteps.CheckRequest)
            {
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
                        BundleHandle = new AssetBundleHandle(_downloadAssetBundleRequest.Url, _options.Bundle, assetBundle, null);
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

    /// <summary>
    /// 从网络加载加密的 AssetBundle 操作
    /// </summary>
    internal sealed class LoadWebEncryptedAssetBundleOperation : BCLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            DataRequest,
            CheckRequest,
            VerifyData,
            LoadBundle,
            CheckResult,
            TryAgain,
            Done,
        }

        private readonly LoadWebAssetBundleOptions _options;
        private readonly DownloadRetryController _downloadRetryController;
        private IDownloadBytesRequest _downloadBytesRequest;
        private IBundleMemoryDecryptor _decryptor;
        private AssetBundleCreateRequest _createRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 创建 LoadWebEncryptedAssetBundleOperation 实例
        /// </summary>
        /// <param name="options">从网络加载 AssetBundle 的配置选项</param>
        public LoadWebEncryptedAssetBundleOperation(LoadWebAssetBundleOptions options)
        {
            _options = options;

            // 注意：网络原因失败后，重新尝试直到成功
            _downloadRetryController = new DownloadRetryController(int.MaxValue, options.DownloadRetryPolicy);
        }
        protected override void InternalStart()
        {
            _steps = ESteps.DataRequest;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DataRequest)
            {
                var decryptor = _options.AssetBundleDecryptor;
                if (decryptor == null)
                {
                    _steps = ESteps.Done;
                    SetError($"{_options.CacheName} decryptor is null.");
                    return;
                }

                if (decryptor is IBundleMemoryDecryptor)
                {
                    _decryptor = decryptor as IBundleMemoryDecryptor;
                    string url = _options.DownloadUrlPolicy.SelectUrl(_options.CandidateUrls);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: 0,
                        watchdogTimeout: _options.WatchdogTimeout);
                    _downloadBytesRequest = _options.DownloadBackend.CreateBytesRequest(args);
                    _downloadBytesRequest.SendRequest();
                    _steps = ESteps.CheckRequest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"{_options.CacheName} does not support '{decryptor.GetType().Name}'.");
                    return;
                }
            }

            if (_steps == ESteps.CheckRequest)
            {
                Progress = _downloadBytesRequest.DownloadProgress;
                if (_downloadBytesRequest.IsDone == false)
                    return;

                if (_downloadBytesRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _options.DownloadUrlPolicy.OnRequestSucceeded(_downloadBytesRequest.Url);
                    _steps = ESteps.VerifyData;
                }
                else
                {
                    string url = _downloadBytesRequest.Url;
                    long httpCode = _downloadBytesRequest.HttpCode;
                    string httpError = _downloadBytesRequest.HttpError;
                    _options.DownloadUrlPolicy.OnRequestFailed(url, httpCode, httpError);
                    if (IsWaitForCompletion == false && _downloadRetryController.CanRetryRequest(url, httpCode, httpError))
                    {
                        _downloadRetryController.StartRetryDelay();
                        _steps = ESteps.TryAgain;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError(_downloadBytesRequest.Error);
                    }
                }
            }

            if (_steps == ESteps.VerifyData)
            {
                // 注意：网络/代理/服务器异常导致内容不完整但请求仍成功
                EFileVerifyResult verifyResult;
                if (_options.DownloadVerifyLevel == EFileVerifyLevel.Low || _options.DownloadVerifyLevel == EFileVerifyLevel.Middle)
                    verifyResult = FileVerifyHelper.VerifyFile(_downloadBytesRequest.Result, _options.Bundle.FileSize, 0);
                else if (_options.DownloadVerifyLevel == EFileVerifyLevel.High)
                    verifyResult = FileVerifyHelper.VerifyFile(_downloadBytesRequest.Result, _options.Bundle.FileSize, _options.Bundle.FileCrc);
                else
                    throw new YooInternalException($"Unexpected verify level: {_options.DownloadVerifyLevel}.");

                if (verifyResult == EFileVerifyResult.Succeed)
                {
                    _steps = ESteps.LoadBundle;
                }
                else
                {
                    string error = $"[WebBundleVerify] Verify failed. Url: '{_downloadBytesRequest.Url}' Level: {_options.DownloadVerifyLevel} Result: {verifyResult}.";
                    YooLogger.LogWarning(error);

                    if (IsWaitForCompletion == false && _downloadRetryController.HasRetriesRemaining())
                    {
                        _downloadRetryController.StartRetryDelay();
                        _steps = ESteps.TryAgain;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError(error);
                    }
                }
            }

            if (_steps == ESteps.LoadBundle)
            {
                LoadResult result = LoadFromMemory(_decryptor, _downloadBytesRequest.Result);
                if (result.Succeeded == false)
                {
                    _steps = ESteps.Done;
                    SetError(result.Error);
                    return;
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_createRequest.isDone == false)
                    return;

                var assetBundle = _createRequest.assetBundle;
                if (assetBundle == null)
                {
                    _steps = ESteps.Done;
                    SetError("Unity engine load failed.");
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
                    BundleHandle = new AssetBundleHandle(_downloadBytesRequest.Url, _options.Bundle, assetBundle, null);
                }
            }

            if (_steps == ESteps.TryAgain)
            {
                // 注意：失败后释放网络请求
                if (_downloadBytesRequest != null)
                {
                    _downloadBytesRequest.Dispose();
                    _downloadBytesRequest = null;
                }

                if (_downloadRetryController.TickRetryDelay())
                {
                    Progress = 0f;
                    _steps = ESteps.DataRequest;
                }
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadBytesRequest != null)
            {
                _downloadBytesRequest.Dispose();
                _downloadBytesRequest = null;
            }
        }

        private LoadResult LoadFromMemory(IBundleMemoryDecryptor decryptor, byte[] fileData)
        {
            var args = new BundleDecryptArgs(_options.Bundle, fileData, null);
            var binaryData = decryptor.GetDecryptedData(args);
            if (binaryData == null)
                return LoadResult.Failure($"{_options.CacheName} decryptor returned null data.");

            _createRequest = AssetBundle.LoadFromMemoryAsync(binaryData);
            return LoadResult.Default();
        }
    }
}
