namespace YooAsset
{
    /// <summary>
    /// 请求Web远端包裹版本操作
    /// </summary>
    internal sealed class RequestWebPackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly RequestWebPackageVersionOptions _options;
        private IDownloadTextRequest _downloadTextRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { get; private set; }


        public RequestWebPackageVersionOperation(RequestWebPackageVersionOptions options)
        {
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_downloadTextRequest == null)
                {
                    string fileName = YooAssetConfiguration.GetPackageVersionFileName(_options.PackageName);
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
                    PackageVersion = _downloadTextRequest.Result;
                    if (TextUtility.ValidateContent(PackageVersion, out string validateError) == false)
                    {
                        _steps = ESteps.Done;
                        SetError($"Web package version file validation failed: {validateError}.");
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
            string url = _options.DownloadUrlPolicy.SelectUrl(urls);

            // 在URL末尾添加时间戳
            if (_options.AppendTimeTicks)
                return $"{url}?{System.DateTime.UtcNow.Ticks}";
            else
                return url;
        }
    }
}