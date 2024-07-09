using System.IO;

namespace YooAsset
{
    internal class LoadEditorPackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly DefaultEditorFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly string _packageHash;
        private DeserializeManifestOperation _deserializer;
        private byte[] _fileData;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        internal LoadEditorPackageManifestOperation(DefaultEditorFileSystem fileSystem, string packageVersion, string packageHash)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadFileData;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadFileData)
            {
                string manifestFilePath = _fileSystem.GetEditorPackageManifestFilePath(_packageVersion);
                if (File.Exists(manifestFilePath))
                {
                    _steps = ESteps.VerifyFileData;
                    _fileData = FileUtility.ReadAllBytes(manifestFilePath);
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found simulation package manifest file : {manifestFilePath}";
                }
            }

            if (_steps == ESteps.VerifyFileData)
            {
                string fileHash = HashUtility.BytesMD5(_fileData);
                if (fileHash == _packageHash)
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to verify simulation package manifest file !";
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializer == null)
                {
                    _deserializer = new DeserializeManifestOperation(_fileData);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _deserializer);
                }

                Progress = _deserializer.Progress;
                if (_deserializer.IsDone == false)
                    return;

                if (_deserializer.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _deserializer.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _deserializer.Error;
                }
            }
        }
    }
}