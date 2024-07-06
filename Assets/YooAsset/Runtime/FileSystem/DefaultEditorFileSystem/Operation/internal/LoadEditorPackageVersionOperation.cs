using System.IO;

namespace YooAsset
{
    internal class LoadEditorPackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadVersion,
            Done,
        }

        private readonly DefaultEditorFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        internal LoadEditorPackageVersionOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadVersion)
            {
                string versionFilePath = _fileSystem.BuildResult.PackageVersionFilePath;
                if (File.Exists(versionFilePath))
                {
                    _steps = ESteps.Done;
                    PackageVersion = FileUtility.ReadAllText(versionFilePath);
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found simulation package version file : {versionFilePath}";
                }
            }
        }
    }
}