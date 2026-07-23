using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统的预下载操作
    /// </summary>
    internal sealed class PreloadWebFileOperation : DownloadFileBaseOperation
    {
        private enum ESteps
        {
            None,
            WaitForAccess,
            CheckExists,
            CreateRequest,
            CheckRequest,
            Done,
        }

        private readonly WebNetworkFileSystem _fileSystem;
        private PreloadConflictController.AccessRequest _conflictRequest;
        private UnityWebRequest _webRequest;
        private UnityWebRequestAsyncOperation _requestOperation;
        private ESteps _steps = ESteps.None;

        internal PreloadWebFileOperation(WebNetworkFileSystem fileSystem, PackageBundle bundle, string url) : base(bundle, url)
        {
            _fileSystem = fileSystem;
        }
        protected override void InternalStart()
        {
            _conflictRequest = _fileSystem.PreloadConflict.CreateAccessRequest(Bundle, PreloadConflictController.EAccessType.CacheDownload);
            _steps = ESteps.WaitForAccess;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.WaitForAccess)
            {
                if (_conflictRequest.TryAcquire() == false)
                    return;

                _steps = ESteps.CheckExists;
            }

            if (_steps == ESteps.CheckExists)
            {
                // 注意：等待同资源包加载结束后缓存状态可能已经变化，需要在发起请求前再次检查。
                if (_fileSystem.IsBundleCached(Bundle))
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.CreateRequest;
                }
            }

            // 创建下载请求
            if (_steps == ESteps.CreateRequest)
            {
                var args = new WebPreloadRequestArgs(Url);
                _webRequest = _fileSystem.PreloadStrategy.CreatePreloadRequest(args);
                if (_webRequest == null)
                    throw new YooInternalException($"{nameof(IWebPreloadStrategy)} returned null web request.");

                _requestOperation = _webRequest.SendWebRequest();
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                LatestReport = DownloadReport.CreateProgress((long)_webRequest.downloadedBytes, _webRequest.downloadProgress);
                Progress = _requestOperation.progress;
                if (_requestOperation.isDone == false)
                    return;

                // 注意：平台插件可能返回 HTTP 成功但并未写入文件缓存，例如请求 URL 与平台缓存规则不匹配。
                // 为保持与原预下载行为一致，此处仅以 HTTP 请求结果作为下载结果。
                long httpCode = _webRequest.responseCode;
                string httpError = _webRequest.error;
                long downloadedBytes = (long)_webRequest.downloadedBytes;
                float downloadProgress = _webRequest.downloadProgress;
                if (CheckRequestSucceed(_webRequest))
                {
                    Progress = 1f;
                    _steps = ESteps.Done;
                    SetResult();

                    // 更新下载报告
                    // 注意：小游戏平台无法返回可靠的下载进度和字节数，成功后按资源包大小回填。
                    // Issue : https://github.com/wechat-miniprogram/minigame-unity-webgl-transform/issues/108#
                    LatestReport = DownloadReport.CreateFinished(
                         httpCode: httpCode,
                         httpError: httpError,
                         downloadedBytes: Bundle.FileSize,
                         downloadProgress: 1f);

                    _fileSystem.DownloadUrlPolicy.OnRequestSucceeded(Url);
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to preload bundle : {Url} Error : {httpError}");

                    // 更新下载报告
                    LatestReport = DownloadReport.CreateFinished(
                         httpCode: httpCode,
                         httpError: httpError,
                         downloadedBytes: downloadedBytes,
                         downloadProgress: downloadProgress);

                    _fileSystem.DownloadUrlPolicy.OnRequestFailed(Url, httpCode, httpError);
                }
            }
        }
        protected override void InternalDispose()
        {
            try
            {
                if (_webRequest != null)
                {
                    _webRequest.Dispose();
                    _webRequest = null;
                    _requestOperation = null;
                }
            }
            finally
            {
                if (_conflictRequest != null)
                {
                    _conflictRequest.Dispose();
                    _conflictRequest = null;
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            string error = $"{GetType().Name} does not support synchronous waiting. Bundle: '{Bundle.BundleName}', Url: '{Url}'.";
            SetError(error);
            YooLogger.LogError(error);
        }

        private bool CheckRequestSucceed(UnityWebRequest request)
        {
#if UNITY_2020_3_OR_NEWER
            return request.result == UnityWebRequest.Result.Success;
#else
            return request.isNetworkError == false && request.isHttpError == false;
#endif
        }
    }
}
