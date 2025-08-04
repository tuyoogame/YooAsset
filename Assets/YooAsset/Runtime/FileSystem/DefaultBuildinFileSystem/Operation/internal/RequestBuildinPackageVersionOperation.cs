using System.IO;

namespace YooAsset
{
    internal class RequestBuildinPackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadPackageVersion,
            RequestPackageVersion,
            CheckResult,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private UnityWebTextRequestOperation _webTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        internal RequestBuildinPackageVersionOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.TryLoadPackageVersion;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadPackageVersion)
            {
                string filePath = _fileSystem.GetBuildinPackageVersionFilePath();
                if (File.Exists(filePath))
                {
                    PackageVersion = File.ReadAllText(filePath);
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    _steps = ESteps.RequestPackageVersion;
                }
            }

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_webTextRequestOp == null)
                {
                    string filePath = _fileSystem.GetBuildinPackageVersionFilePath();
                    string url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webTextRequestOp = new UnityWebTextRequestOperation(url, 60);
                    _webTextRequestOp.StartOperation();
                    AddChildOperation(_webTextRequestOp);
                }

                _webTextRequestOp.UpdateOperation();
                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = _webTextRequestOp.Result;
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webTextRequestOp.Error;
                }
            }

            if (_steps == ESteps.CheckResult)
            {
                if (string.IsNullOrEmpty(PackageVersion))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Buildin package version file content is empty !";
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }
    }
}