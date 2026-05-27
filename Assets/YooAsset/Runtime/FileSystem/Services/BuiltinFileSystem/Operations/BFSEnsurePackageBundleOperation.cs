
namespace YooAsset
{
    internal sealed class BFSEnsurePackageBundleOperation : FSEnsurePackageBundleOperation
    {
        private enum ESteps
        {
            None,
            CheckUnpack,
            UnpackFile,
            CheckFilePath,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private readonly FSEnsurePackageBundleOptions _options;
        private FSDownloadBundleOperation _unpackFileOp;
        private ESteps _steps = ESteps.None;

        internal BFSEnsurePackageBundleOperation(BuiltinFileSystem fileSystem, FSEnsurePackageBundleOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckUnpack;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckUnpack)
            {
                if (_fileSystem.IsUnpackBundle(_options.Bundle))
                {
                    if (_fileSystem.UnpackBundleCache.IsCached(_options.Bundle.BundleGuid))
                    {
                        _steps = ESteps.CheckFilePath;
                    }
                    else
                    {
                        _steps = ESteps.UnpackFile;
                    }
                }
                else
                {
                    BundleFilePath = _fileSystem.GetBuiltinBundleFilePath(_options.Bundle);
                    _steps = ESteps.Done;
                    SetResult();
                }
            }

            if (_steps == ESteps.UnpackFile)
            {
                if (_unpackFileOp == null)
                {
                    var options = new FSDownloadBundleOptions(_options.Bundle, 0);
                    _unpackFileOp = _fileSystem.DownloadBundleAsync(options);
                    _unpackFileOp.StartOperation();
                    AddChildOperation(_unpackFileOp);
                }

                if (IsWaitForCompletion)
                    _unpackFileOp.WaitForCompletion();

                _unpackFileOp.UpdateOperation();
                Progress = _unpackFileOp.Progress;
                if (_unpackFileOp.IsDone == false)
                    return;

                if (_unpackFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CheckFilePath;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_unpackFileOp.Error);
                }
            }

            if (_steps == ESteps.CheckFilePath)
            {
                string filePath = _fileSystem.UnpackBundleCache.GetCacheFilePath(_options.Bundle.BundleGuid);
                if (string.IsNullOrEmpty(filePath))
                {
                    _steps = ESteps.Done;
                    SetError($"Bundle '{_options.Bundle.BundleName}' cache file path not found.");
                }
                else
                {
                    _steps = ESteps.Done;
                    BundleFilePath = filePath;
                    SetResult();
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
