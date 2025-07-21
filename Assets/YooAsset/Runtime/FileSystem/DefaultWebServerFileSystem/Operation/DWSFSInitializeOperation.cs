
namespace YooAsset
{
    internal class DWSFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultWebServerFileSystem _fileSystem;
        private LoadWebServerCatalogFileOperation _loadCatalogFileOp;
        private ESteps _steps = ESteps.None;


        public DWSFSInitializeOperation(DefaultWebServerFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.LoadCatalogFile;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadCatalogFile)
            {
                if (_loadCatalogFileOp == null)
                {
                    _loadCatalogFileOp = new LoadWebServerCatalogFileOperation(_fileSystem, 60);
                    _loadCatalogFileOp.StartOperation();
                    AddChildOperation(_loadCatalogFileOp);
                }

                _loadCatalogFileOp.UpdateOperation();
                if (_loadCatalogFileOp.IsDone == false)
                    return;

                if (_loadCatalogFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCatalogFileOp.Error;
                }
            }
        }
    }
}