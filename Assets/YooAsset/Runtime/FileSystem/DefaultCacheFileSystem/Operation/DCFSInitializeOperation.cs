
namespace YooAsset
{
    internal class DCFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            CheckAppFootPrint,
            SearchCacheFiles,
            VerifyCacheFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSytem;
        private SearchCacheFilesOperation _searchCacheFilesOp;
        private VerifyCacheFilesOperation _verifyCacheFilesOp;
        private ESteps _steps = ESteps.None;


        internal DCFSInitializeOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSytem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckAppFootPrint;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckAppFootPrint)
            {
                var appFootPrint = new ApplicationFootPrint(_fileSytem);
                appFootPrint.Load(_fileSytem.PackageName);

                // 如果水印发生变化，则说明覆盖安装后首次打开游戏
                if (appFootPrint.IsDirty())
                {
                    _fileSytem.DeleteAllManifestFiles();
                    appFootPrint.Coverage(_fileSytem.PackageName);
                    YooLogger.Warning("Delete manifest files when application foot print dirty !");
                }

                _steps = ESteps.SearchCacheFiles;
            }

            if (_steps == ESteps.SearchCacheFiles)
            {
                if (_searchCacheFilesOp == null)
                {
                    _searchCacheFilesOp = new SearchCacheFilesOperation(_fileSytem);
                    OperationSystem.StartOperation(_fileSytem.PackageName, _searchCacheFilesOp);
                }

                Progress = _searchCacheFilesOp.Progress;
                if (_searchCacheFilesOp.IsDone == false)
                    return;

                _steps = ESteps.VerifyCacheFiles;
            }

            if (_steps == ESteps.VerifyCacheFiles)
            {
                if (_verifyCacheFilesOp == null)
                {
                    _verifyCacheFilesOp = new VerifyCacheFilesOperation(_fileSytem, _searchCacheFilesOp.Result);
                    OperationSystem.StartOperation(_fileSytem.PackageName, _verifyCacheFilesOp);
                }

                Progress = _verifyCacheFilesOp.Progress;
                if (_verifyCacheFilesOp.IsDone == false)
                    return;

                if (_verifyCacheFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    YooLogger.Log($"Package '{_fileSytem.PackageName}' cached files count : {_fileSytem.FileCount}");
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _verifyCacheFilesOp.Error;
                }
            }
        }
    }
}