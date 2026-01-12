
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件系统的查询包裹版本操作
    /// </summary>
    internal sealed class EFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            LoadPackageVersion,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private LoadEditorPackageVersionOperation _loadEditorPackageVersionOp;
        private ESteps _steps = ESteps.None;


        internal EFSRequestPackageVersionOperation(EditorFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadPackageVersion;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadPackageVersion)
            {
                if (_loadEditorPackageVersionOp == null)
                {
                    _loadEditorPackageVersionOp = new LoadEditorPackageVersionOperation(_fileSystem);
                    _loadEditorPackageVersionOp.StartOperation();
                    AddChildOperation(_loadEditorPackageVersionOp);
                }

                _loadEditorPackageVersionOp.UpdateOperation();
                if (_loadEditorPackageVersionOp.IsDone == false)
                    return;

                if (_loadEditorPackageVersionOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _loadEditorPackageVersionOp.PackageVersion;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadEditorPackageVersionOp.Error);
                }
            }
        }
    }
}