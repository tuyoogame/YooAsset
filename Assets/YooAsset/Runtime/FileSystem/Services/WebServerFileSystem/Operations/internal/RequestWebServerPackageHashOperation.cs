
namespace YooAsset
{
    /// <summary>
    /// 请求Web服务端包裹哈希操作
    /// </summary>
    internal sealed class RequestWebServerPackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageHash,
            Done,
        }

        private readonly WebServerFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private IDownloadTextRequest _downloadTextRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { get; private set; }


        public RequestWebServerPackageHashOperation(WebServerFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
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
                    string filePath = _fileSystem.GetWebPackageHashFilePath(_packageVersion);
                    string url = DownloadUrlHelper.ToLocalFileUrl(filePath);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: _timeout,
                        watchdogTimeout: 0);
                    _downloadTextRequest = _fileSystem.DownloadBackend.CreateTextRequest(args);
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
                        SetError($"Web server package hash file validation failed: {validateError}.");
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
    }
}