namespace YooAsset
{
    /// <summary>
    /// 加载Web远端包裹清单文件操作
    /// </summary>
    internal sealed class LoadWebPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly LoadWebPackageManifestOptions _options;
        private IDownloadBytesRequest _downloadBytesRequest;
        private DeserializeManifestOperation _deserializeManifestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { get; private set; }


        internal LoadWebPackageManifestOperation(LoadWebPackageManifestOptions options)
        {
            _options = options;
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
                    string fileName = YooAssetConfiguration.GetManifestBinaryFileName(_options.PackageName, _options.PackageVersion);
                    string url = GetRequestUrl(fileName);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: _options.Timeout,
                        watchdogTimeout: 0);
                    _downloadBytesRequest = _options.DownloadBackend.CreateBytesRequest(args);
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
                    _options.DownloadUrlPolicy.OnRequestFailed(_downloadBytesRequest.Url, _downloadBytesRequest.HttpCode, _downloadBytesRequest.HttpError);
                }
            }

            if (_steps == ESteps.VerifyFileData)
            {
                if (PackageManifestHelper.VerifyManifestData(_downloadBytesRequest.Result, _options.PackageHash))
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError("Failed to verify web package manifest file.");
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializeManifestOp == null)
                {
                    _deserializeManifestOp = new DeserializeManifestOperation(_options.ManifestDecryptor, _downloadBytesRequest.Result);
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
            return $"PackageVersion: {_options.PackageVersion} PackageHash: {_options.PackageHash}";
        }

        private string GetRequestUrl(string fileName)
        {
            var urls = _options.RemoteService.GetRemoteUrls(fileName);
            return _options.DownloadUrlPolicy.SelectUrl(urls);
        }
    }
}