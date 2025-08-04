using System.IO;

namespace YooAsset
{
    internal class RequestBuildinPackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            TryLoadPackageHash,
            RequestPackageHash,
            CheckResult,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly string _packageVersion;
        private UnityWebTextRequestOperation _webTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { private set; get; }


        internal RequestBuildinPackageHashOperation(DefaultBuildinFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.TryLoadPackageHash;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.TryLoadPackageHash)
            {
                string filePath = _fileSystem.GetBuildinPackageHashFilePath(_packageVersion);
                if (File.Exists(filePath))
                {
                    PackageHash = File.ReadAllText(filePath);
                    _steps = ESteps.CheckResult;
                }
                else
                {
                    _steps = ESteps.RequestPackageHash;
                }
            }

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_webTextRequestOp == null)
                {
                    string filePath = _fileSystem.GetBuildinPackageHashFilePath(_packageVersion);
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
                    PackageHash = _webTextRequestOp.Result;
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
                if (string.IsNullOrEmpty(PackageHash))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Buildin package hash file content is empty !";
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