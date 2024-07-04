
namespace YooAsset
{
    internal sealed class DBFSClearAllBundleFilesOperation : FSClearAllBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearUnpackFileSystem,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private FSClearAllBundleFilesOperation _unpackClearAllBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal DBFSClearAllBundleFilesOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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
                if (_unpackClearAllBundleFilesOp == null)
                    _unpackClearAllBundleFilesOp = _fileSystem.UnpackFileSystem.ClearAllBundleFilesAsync();

                Progress = _unpackClearAllBundleFilesOp.Progress;
                if (_unpackClearAllBundleFilesOp.IsDone == false)
                    return;

                _steps = ESteps.Done;
                Status = _unpackClearAllBundleFilesOp.Status;
                Error = _unpackClearAllBundleFilesOp.Error;
            }
        }
    }
}