using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 请求内置包裹版本操作
    /// </summary>
    internal sealed class RequestBuiltinPackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadPackageVersion,
            RequestPackageVersion,
            CheckResult,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private IDownloadTextRequest _downloadTextRequest;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { get; private set; }


        internal RequestBuiltinPackageVersionOperation(BuiltinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.TryLoadPackageVersion;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadPackageVersion)
            {
                string filePath = _fileSystem.GetBuiltinPackageVersionFilePath();
                if (File.Exists(filePath))
                {
                    try
                    {
                        PackageVersion = File.ReadAllText(filePath);
                        _steps = ESteps.CheckResult;
                    }
                    catch (System.Exception ex)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to read builtin package version file: {ex.Message}.");
                    }
                }
                else
                {
                    _steps = ESteps.RequestPackageVersion;
                }
            }

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_downloadTextRequest == null)
                {
                    string filePath = _fileSystem.GetBuiltinPackageVersionFilePath();
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
                    PackageVersion = _downloadTextRequest.Result;
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
                if (TextUtility.ValidateContent(PackageVersion, out string validateError) == false)
                {
                    _steps = ESteps.Done;
                    SetError($"Builtin package version file validation failed: {validateError}.");
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