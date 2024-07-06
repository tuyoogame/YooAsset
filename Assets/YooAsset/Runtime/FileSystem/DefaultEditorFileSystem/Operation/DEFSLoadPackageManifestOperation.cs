
namespace YooAsset
{
    internal class DEFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            LoadEditorPackageManifest,
            Done,
        }

        private readonly DefaultEditorFileSystem _fileSystem;
        private LoadEditorPackageManifestOperation _loadEditorPackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal DEFSLoadPackageManifestOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadEditorPackageManifest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadEditorPackageManifest)
            {
                if (_loadEditorPackageManifestOp == null)
                {
                    _loadEditorPackageManifestOp = new LoadEditorPackageManifestOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadEditorPackageManifestOp);
                }

                Progress = _loadEditorPackageManifestOp.Progress;
                if (_loadEditorPackageManifestOp.IsDone == false)
                    return;

                if (_loadEditorPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadEditorPackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadEditorPackageManifestOp.Error;
                }
            }
        }
    }
}