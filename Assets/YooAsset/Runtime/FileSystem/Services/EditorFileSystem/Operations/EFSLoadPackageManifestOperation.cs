
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件系统的加载包裹清单操作
    /// </summary>
    internal sealed class EFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private enum ESteps
        {
            None,
            LoadPackageHash,
            LoadPackageManifest,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private readonly string _packageVersion;
        private LoadEditorPackageHashOperation _loadEditorPackageHashOp;
        private LoadEditorPackageManifestOperation _loadEditorPackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal EFSLoadPackageManifestOperation(EditorFileSystem fileSystem, string packageVersion)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadPackageHash;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadPackageHash)
            {
                if (_loadEditorPackageHashOp == null)
                {
                    _loadEditorPackageHashOp = new LoadEditorPackageHashOperation(_fileSystem, _packageVersion);
                    _loadEditorPackageHashOp.StartOperation();
                    AddChildOperation(_loadEditorPackageHashOp);
                }

                _loadEditorPackageHashOp.UpdateOperation();
                if (_loadEditorPackageHashOp.IsDone == false)
                    return;

                if (_loadEditorPackageHashOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.LoadPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadEditorPackageHashOp.Error);
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadEditorPackageManifestOp == null)
                {
                    string packageHash = _loadEditorPackageHashOp.PackageHash;
                    _loadEditorPackageManifestOp = new LoadEditorPackageManifestOperation(_fileSystem, _packageVersion, packageHash);
                    _loadEditorPackageManifestOp.StartOperation();
                    AddChildOperation(_loadEditorPackageManifestOp);
                }

                _loadEditorPackageManifestOp.UpdateOperation();
                Progress = _loadEditorPackageManifestOp.Progress;
                if (_loadEditorPackageManifestOp.IsDone == false)
                    return;

                if (_loadEditorPackageManifestOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    SetResult();
                    Manifest = _loadEditorPackageManifestOp.Manifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadEditorPackageManifestOp.Error);
                }
            }
        }
    }
}