
namespace YooAsset
{
    internal sealed class EFSEnsurePackageBundleOperation : FSEnsurePackageBundleOperation
    {
        private enum ESteps
        {
            None,
            CheckCache,
            DownloadFile,
            CheckFilePath,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private readonly FSEnsurePackageBundleOptions _options;
        private FSDownloadBundleOperation _downloadFileOp;
        private ESteps _steps = ESteps.None;

        internal EFSEnsurePackageBundleOperation(EditorFileSystem fileSystem, FSEnsurePackageBundleOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckCache;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckCache)
            {
                if (_fileSystem.BundleCache.IsCached(_options.Bundle.BundleGuid))
                {
                    _steps = ESteps.CheckFilePath;
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_downloadFileOp == null)
                {
                    var options = new FSDownloadBundleOptions(_options.Bundle, 0);
                    _downloadFileOp = _fileSystem.DownloadBundleAsync(options);
                    _downloadFileOp.StartOperation();
                    AddChildOperation(_downloadFileOp);
                }

                if (IsWaitForCompletion)
                    _downloadFileOp.WaitForCompletion();

                _downloadFileOp.UpdateOperation();
                Progress = _downloadFileOp.Progress;
                if (_downloadFileOp.IsDone == false)
                    return;

                if (_downloadFileOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CheckFilePath;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_downloadFileOp.Error);
                }
            }

            if (_steps == ESteps.CheckFilePath)
            {
                string filePath = EditorFileSystemHelper.GetEditorFilePath(_options.Bundle);
                if (string.IsNullOrEmpty(filePath))
                {
                    _steps = ESteps.Done;
                    SetError($"Editor file path is empty for bundle '{_options.Bundle.BundleName}'.");
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
