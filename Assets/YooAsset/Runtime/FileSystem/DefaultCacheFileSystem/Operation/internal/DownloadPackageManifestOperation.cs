using System.IO;

namespace YooAsset
{
    internal class DownloadPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckExist,
            DownloadFile,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private IDownloadFileRequest _webFileRequestOp;
        private int _requestCount = 0;
        private ESteps _steps = ESteps.None;


        internal DownloadPackageManifestOperation(DefaultCacheFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalStart()
        {
            _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(DownloadPackageManifestOperation));
            _steps = ESteps.CheckExist;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckExist)
            {
                string filePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_webFileRequestOp == null)
                {
                    string savePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                    string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_fileSystem.PackageName, _packageVersion);
                    string webURL = GetDownloadRequestURL(fileName);
                    int watchdogTime = _fileSystem.DownloadWatchDogTime;
                    var args = new DownloadFileRequestArgs(webURL, savePath, _timeout, watchdogTime);
                    _webFileRequestOp = _fileSystem.DownloadBackend.CreateFileRequest(args);
                    _webFileRequestOp.SendRequest();
                }

                if (_webFileRequestOp.IsDone == false)
                    return;

                if (_webFileRequestOp.Status == EDownloadRequestStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webFileRequestOp.Error;
                    WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(DownloadPackageManifestOperation));
                }
            }
        }

        private string GetDownloadRequestURL(string fileName)
        {
            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                return _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
            else
                return _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);
        }
    }
}