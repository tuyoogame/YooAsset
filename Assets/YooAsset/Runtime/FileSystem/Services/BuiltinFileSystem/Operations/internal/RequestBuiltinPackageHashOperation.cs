using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 请求内置包裹哈希操作
    /// </summary>
    internal sealed class RequestBuiltinPackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadPackageHash,
            RequestPackageHash,
            CheckResult,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly string _packageVersion;
        private IDownloadTextRequest _downloadTextRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { get; private set; }


        internal RequestBuiltinPackageHashOperation(BuiltinFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.TryLoadPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadPackageHash)
            {
                string filePath = _fileSystem.GetBuiltinPackageHashFilePath(_packageVersion);
                if (File.Exists(filePath))
                {
                    try
                    {
                        PackageHash = File.ReadAllText(filePath);
                        _steps = ESteps.CheckResult;
                    }
                    catch (System.Exception ex)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to read builtin package hash file: {ex.Message}.");
                    }
                }
                else
                {
                    _steps = ESteps.RequestPackageHash;
                }
            }

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_downloadTextRequest == null)
                {
                    string filePath = _fileSystem.GetBuiltinPackageHashFilePath(_packageVersion);
                    string url = DownloadUrlHelper.ToLocalFileUrl(filePath);
                    var args = new DownloadDataRequestArgs(
                        url: url,
                        timeout: 60,
                        watchdogTimeout: 0);
                    _downloadTextRequest = _fileSystem.DownloadBackend.CreateTextRequest(args);
                    _downloadTextRequest.SendRequest();
                }

                if (_downloadTextRequest.IsDone == false)
                    return;

                if (_downloadTextRequest.Status == EDownloadRequestStatus.Succeeded)
                {
                    PackageHash = _downloadTextRequest.Result;
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadTextRequest.Error);
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (TextUtility.ValidateContent(PackageHash, out string validateError) == false)
                {
                    _steps = ESteps.Done;
                    SetError($"Builtin package hash file validation failed: {validateError}.");
                }
                else
                {
                    _steps = ESteps.Done;
                    SetResult();
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