
namespace YooAsset
{
    /// <summary>
    /// 加载Web服务端包裹清单文件操作
    /// </summary>
    internal sealed class LoadWebServerPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly WebServerFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly string _packageHash;
        private readonly int _timeout;
        private IDownloadBytesRequest _downloadBytesRequest;
        private DeserializeManifestOperation _deserializeManifestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { get; private set; }


        internal LoadWebServerPackageManifestOperation(WebServerFileSystem fileSystem, string packageVersion, string packageHash, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
            _timeout = timeout;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RequestFileData;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestFileData)
            {
                if (_downloadBytesRequest == null)
                {
                    string filePath = _fileSystem.GetWebPackageManifestFilePath(_packageVersion);
                    string url = DownloadUrlHelper.ToLocalFileUrl(filePath);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: _timeout,
                        watchdogTimeout: 0);
                    _downloadBytesRequest = _fileSystem.DownloadBackend.CreateBytesRequest(args);
                    _downloadBytesRequest.SendRequest();
                }

                Progress = _downloadBytesRequest.DownloadProgress;
                if (_downloadBytesRequest.IsDone == false)
                    return;

                if (_downloadBytesRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _steps = ESteps.VerifyFileData;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadBytesRequest.Error);
                }
            }

            if (_steps == ESteps.VerifyFileData)
            {
                if (PackageManifestHelper.VerifyManifestData(_downloadBytesRequest.Result, _packageHash))
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError("Failed to verify web server package manifest file.");
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializeManifestOp == null)
                {
                    _deserializeManifestOp = new DeserializeManifestOperation(_fileSystem.ManifestDecryptor, _downloadBytesRequest.Result);
                    _deserializeManifestOp.StartOperation();
                    AddChildOperation(_deserializeManifestOp);
                }

                _deserializeManifestOp.UpdateOperation();
                Progress = _deserializeManifestOp.Progress;
                if (_deserializeManifestOp.IsDone == false)
                    return;

                if (_deserializeManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    Manifest = _deserializeManifestOp.Manifest;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_deserializeManifestOp.Error);
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
        protected override string InternalGetDescription()
        {
            return $"PackageVersion: {_packageVersion} PackageHash: {_packageHash}";
        }
    }
}