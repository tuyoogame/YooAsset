
namespace YooAsset
{
    internal class DEFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            LoadPackageVersion,
            Done,
        }

        private readonly DefaultEditorFileSystem _fileSystem;
        private LoadEditorPackageVersionOperation _loadEditorPackageVersionOp;
        private ESteps _steps = ESteps.None;


        internal DEFSRequestPackageVersionOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadPackageVersion)
            {
                if (_loadEditorPackageVersionOp == null)
                {
                    _loadEditorPackageVersionOp = new LoadEditorPackageVersionOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadEditorPackageVersionOp);
                }

                if (_loadEditorPackageVersionOp.IsDone == false)
                    return;

                if (_loadEditorPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _loadEditorPackageVersionOp.PackageVersion;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadEditorPackageVersionOp.Error;
                }
            }
        }
    }
}