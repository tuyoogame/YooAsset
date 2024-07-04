
namespace YooAsset
{
    internal partial class DWFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private LoadWebCatalogFileOperation _loadCatalogFileOp;
        private ESteps _steps = ESteps.None;


        public DWFSInitializeOperation(DefaultWebFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadCatalogFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadCatalogFile)
            {
                if (_loadCatalogFileOp == null)
                {
                    _loadCatalogFileOp = new LoadWebCatalogFileOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCatalogFileOp);
                }

                if (_loadCatalogFileOp.IsDone == false)
                    return;

                // 说明：应用程序内不一定存在序列化文件
                // 注意：总是返回成功
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}