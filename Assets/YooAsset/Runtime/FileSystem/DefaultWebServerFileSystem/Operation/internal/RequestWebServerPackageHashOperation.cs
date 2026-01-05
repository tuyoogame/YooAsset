
namespace YooAsset
{
    internal class RequestWebServerPackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageHash,
            Done,
        }

        private readonly DefaultWebServerFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private IDownloadTextRequest _webTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { private set; get; }


        public RequestWebServerPackageHashOperation(DefaultWebServerFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestPackageHash;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_webTextRequestOp == null)
                {
                    string filePath = _fileSystem.GetWebPackageHashFilePath(_packageVersion);
                    string url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    var args = new DownloadDataRequestArgs(url, _timeout, 0);
                    _webTextRequestOp = _fileSystem.DownloadBackend.CreateTextRequest(args);
                    _webTextRequestOp.SendRequest();
                }

                Progress = _webTextRequestOp.DownloadProgress;
                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EDownloadRequestStatus.Succeed)
                {
                    PackageHash = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageHash))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Web server package hash file content is empty !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webTextRequestOp.Error;
                }
            }
        }
    }
}