using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 加载内置包裹清单文件操作
    /// </summary>
    internal sealed class LoadBuiltinPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadFileData,
            RequestFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly string _packageHash;
        private IDownloadBytesRequest _downloadBytesRequest;
        private DeserializeManifestOperation _deserializeManifestOp;
        private byte[] _fileData;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { get; private set; }


        internal LoadBuiltinPackageManifestOperation(BuiltinFileSystem fileSystem, string packageVersion, string packageHash)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.TryLoadFileData;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadFileData)
            {
                string filePath = _fileSystem.GetBuiltinPackageManifestFilePath(_packageVersion);
                if (File.Exists(filePath))
                {
                    try
                    {
                        _fileData = File.ReadAllBytes(filePath);
                        _steps = ESteps.VerifyFileData;
                    }
                    catch (System.Exception ex)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to read builtin package manifest file: {ex.Message}.");
                    }
                }
                else
                {
                    _steps = ESteps.RequestFileData;
                }
            }

            if (_steps == ESteps.RequestFileData)
            {
                if (_downloadBytesRequest == null)
                {
                    string filePath = _fileSystem.GetBuiltinPackageManifestFilePath(_packageVersion);
                    string url = DownloadUrlHelper.ToLocalFileUrl(filePath);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: 60,
                        watchdogTimeout: 0);
                    _downloadBytesRequest = _fileSystem.DownloadBackend.CreateBytesRequest(args);
                    _downloadBytesRequest.SendRequest();
                }

                if (_downloadBytesRequest.IsDone == false)
                    return;

                if (_downloadBytesRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    _fileData = _downloadBytesRequest.Result;
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
                if (PackageManifestHelper.VerifyManifestData(_fileData, _packageHash))
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError("Failed to verify builtin package manifest file.");
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializeManifestOp == null)
                {
                    _deserializeManifestOp = new DeserializeManifestOperation(_fileSystem.ManifestDecryptor, _fileData);
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