
namespace YooAsset
{
    internal class RequestWebServerPackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly DefaultWebServerFileSystem _fileSystem;
        private readonly int _timeout;
        private IDownloadTextRequest _webTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        internal RequestWebServerPackageVersionOperation(DefaultWebServerFileSystem fileSystem, int timeout)
        {
            _fileSystem = fileSystem;
            _timeout = timeout;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_webTextRequestOp == null)
                {
                    string filePath = _fileSystem.GetWebPackageVersionFilePath();
                    string url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    var args = new DownloadDataRequestArgs(url, _timeout, 0);
                    _webTextRequestOp = _fileSystem.DownloadBackend.CreateTextRequest(args);
                    _webTextRequestOp.SendRequest();
                }

                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EDownloadRequestStatus.Succeed)
                {
                    PackageVersion = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Web server package version file content is empty !";
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