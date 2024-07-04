
namespace YooAsset
{
    internal sealed class DBFSClearUnusedBundleFilesOperation : FSClearUnusedBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearUnpackFileSystem,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly PackageManifest _manifest;
        private FSClearUnusedBundleFilesOperation _unpackClearUnusedBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal DBFSClearUnusedBundleFilesOperation(DefaultBuildinFileSystem fileSystem, PackageManifest manifest)
        {
            _fileSystem = fileSystem;
            _manifest = manifest;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearUnpackFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearUnpackFileSystem)
            {
                if (_unpackClearUnusedBundleFilesOp == null)
                    _unpackClearUnusedBundleFilesOp = _fileSystem.UnpackFileSystem.ClearUnusedBundleFilesAsync(_manifest);

                Progress = _unpackClearUnusedBundleFilesOp.Progress;
                if (_unpackClearUnusedBundleFilesOp.IsDone == false)
                    return;

                _steps = ESteps.Done;
                Status = _unpackClearUnusedBundleFilesOp.Status;
                Error = _unpackClearUnusedBundleFilesOp.Error;
            }
        }
    }
}