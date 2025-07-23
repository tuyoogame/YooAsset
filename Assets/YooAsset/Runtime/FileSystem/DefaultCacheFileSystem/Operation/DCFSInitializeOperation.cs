
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
            CreateDownloadCenter,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private SearchCacheFilesOperation _searchCacheFilesOp;
        private VerifyCacheFilesOperation _verifyCacheFilesOp;
        private ESteps _steps = ESteps.None;


        internal DCFSInitializeOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
#if UNITY_WEBGL
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = $"{nameof(DefaultCacheFileSystem)} is not support WEBGL platform !";
#else
            _steps = ESteps.CheckAppFootPrint;
#endif
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckAppFootPrint)
            {
                var appFootPrint = new ApplicationFootPrint(_fileSystem);
                appFootPrint.Load(_fileSystem.PackageName);

                // 如果水印发生变化，则说明覆盖安装后首次打开游戏
                if (appFootPrint.IsDirty())
                {
                    if (_fileSystem.InstallClearMode == EOverwriteInstallClearMode.None)
                    {
                        YooLogger.Warning("Do nothing when overwrite install application !");
                    }
                    else if (_fileSystem.InstallClearMode == EOverwriteInstallClearMode.ClearAllCacheFiles)
                    {
                        _fileSystem.DeleteAllBundleFiles();
                        _fileSystem.DeleteAllManifestFiles();
                        YooLogger.Warning("Delete all cache files when overwrite install application !");
                    }
                    else if (_fileSystem.InstallClearMode == EOverwriteInstallClearMode.ClearAllBundleFiles)
                    {
                        _fileSystem.DeleteAllBundleFiles();
                        YooLogger.Warning("Delete all bundle files when overwrite install application !");
                    }
                    else if (_fileSystem.InstallClearMode == EOverwriteInstallClearMode.ClearAllManifestFiles)
                    {
                        _fileSystem.DeleteAllManifestFiles();
                        YooLogger.Warning("Delete all manifest files when overwrite install application !");
                    }
                    else
                    {
                        throw new System.NotImplementedException(_fileSystem.InstallClearMode.ToString());
                    }

                    appFootPrint.Coverage(_fileSystem.PackageName);
                }

                _steps = ESteps.SearchCacheFiles;
            }

            if (_steps == ESteps.SearchCacheFiles)
            {
                if (_searchCacheFilesOp == null)
                {
                    _searchCacheFilesOp = new SearchCacheFilesOperation(_fileSystem);
                    _searchCacheFilesOp.StartOperation();
                    AddChildOperation(_searchCacheFilesOp);
                }

                _searchCacheFilesOp.UpdateOperation();
                Progress = _searchCacheFilesOp.Progress;
                if (_searchCacheFilesOp.IsDone == false)
                    return;

                _steps = ESteps.VerifyCacheFiles;
            }

            if (_steps == ESteps.VerifyCacheFiles)
            {
                if (_verifyCacheFilesOp == null)
                {
                    _verifyCacheFilesOp = new VerifyCacheFilesOperation(_fileSystem, _searchCacheFilesOp.Result);
                    _verifyCacheFilesOp.StartOperation();
                    AddChildOperation(_verifyCacheFilesOp);
                }

                _verifyCacheFilesOp.UpdateOperation();
                Progress = _verifyCacheFilesOp.Progress;
                if (_verifyCacheFilesOp.IsDone == false)
                    return;

                if (_verifyCacheFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CreateDownloadCenter;
                    YooLogger.Log($"Package '{_fileSystem.PackageName}' '{_fileSystem.GetType().Name}' cached files count : {_fileSystem.FileCount}");
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _verifyCacheFilesOp.Error;
                }
            }

            if (_steps == ESteps.CreateDownloadCenter)
            {
                // 注意：下载中心作为独立任务运行！
                if (_fileSystem.DownloadCenter == null)
                {
                    _fileSystem.DownloadCenter = new DownloadCenterOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _fileSystem.DownloadCenter);
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}