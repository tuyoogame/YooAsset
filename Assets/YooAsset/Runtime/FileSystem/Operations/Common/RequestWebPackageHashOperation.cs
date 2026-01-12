namespace YooAsset
{
    /// <summary>
    /// 请求Web远端包裹哈希操作
    /// </summary>
    internal sealed class RequestWebPackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageHash,
            Done,
        }

        private readonly RequestWebPackageHashOptions _options;
        private IDownloadTextRequest _downloadTextRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { get; private set; }


        public RequestWebPackageHashOperation(RequestWebPackageHashOptions options)
        {
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RequestPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_downloadTextRequest == null)
                {
                    string fileName = YooAssetConfiguration.GetPackageHashFileName(_options.PackageName, _options.PackageVersion);
                    string url = GetRequestUrl(fileName);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: _options.Timeout,
                        watchdogTimeout: 0);
                    _downloadTextRequest = _options.DownloadBackend.CreateTextRequest(args);
                    _downloadTextRequest.SendRequest();
                }

                Progress = _downloadTextRequest.DownloadProgress;
                if (_downloadTextRequest.IsDone == false)
                    return;

                if (_downloadTextRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    PackageHash = _downloadTextRequest.Result;
                    if (TextUtility.ValidateContent(PackageHash, out string validateError) == false)
                    {
                        _steps = ESteps.Done;
                        SetError($"Web package hash file validation failed: {validateError}.");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetResult();
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadTextRequest.Error);
                    _options.DownloadUrlPolicy.OnRequestFailed(_downloadTextRequest.Url, _downloadTextRequest.HttpCode, _downloadTextRequest.HttpError);
                }
            }
        }
        protected override void InternalDispose()
        {
            if (_downloadTextRequest != null)
            {
                _downloadTextRequest.Dispose();
                _downloadTextRequest = null;
            }
        }

        private string GetRequestUrl(string fileName)
        {
            var urls = _options.RemoteService.GetRemoteUrls(fileName);
            return _options.DownloadUrlPolicy.SelectUrl(urls);
        }
    }
}