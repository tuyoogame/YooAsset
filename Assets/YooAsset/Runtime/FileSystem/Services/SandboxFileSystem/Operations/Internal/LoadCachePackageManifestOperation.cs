using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 加载缓存包裹清单文件操作
    /// </summary>
    internal sealed class LoadCachePackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly string _packageHash;
        private DeserializeManifestOperation _deserializeManifestOp;
        private byte[] _fileData;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { get; private set; }


        internal LoadCachePackageManifestOperation(SandboxFileSystem fileSystem, string packageVersion, string packageHash)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadFileData;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadFileData)
            {
                string manifestFilePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                if (File.Exists(manifestFilePath))
                {
                    try
                    {
                        _fileData = File.ReadAllBytes(manifestFilePath);
                        _steps = ESteps.VerifyFileData;
                    }
                    catch (System.Exception ex)
                    {
                        _steps = ESteps.Done;
                        SetError($"Failed to read cache package manifest file: {ex.Message}.");
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Could not find cache manifest file: '{manifestFilePath}'.");
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
                    SetError("Failed to verify cache package manifest file.");
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
        protected override string InternalGetDescription()
        {
            return $"PackageVersion: {_packageVersion} PackageHash: {_packageHash}";
        }
    }
}