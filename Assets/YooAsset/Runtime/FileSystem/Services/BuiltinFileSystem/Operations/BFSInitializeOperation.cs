
namespace YooAsset
{
    /// <summary>
    /// 内置文件系统的初始化操作
    /// </summary>
    internal sealed class BFSInitializeOperation : FSInitializeOperation
    {
        private enum ESteps
        {
            None,
            CheckPlatform,
            CheckAppFootprint,
            CopyPackageManifest,
            InitializeBuiltinBundleCache,
            InitializeUnpackBundleCache,
            CreateScheduler,
            Done,
        }

        private readonly BuiltinFileSystem _fileSystem;
        private BCInitializeOperation _initializeBuiltinBundleCacheOp;
        private BCInitializeOperation _initializeUnpackBundleCacheOp;
        private CopyBuiltinPackageManifestOperation _copyBuiltinPackageManifestOp;
        private ESteps _steps = ESteps.None;

        internal BFSInitializeOperation(BuiltinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.CheckPlatform;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckPlatform)
            {
#if UNITY_WEBGL
                _steps = ESteps.Done;
                SetError($"{nameof(BuiltinFileSystem)} does not support the WebGL platform.");
#else
                _steps = ESteps.CheckAppFootprint;
#endif
            }

            if (_steps == ESteps.CheckAppFootprint)
            {
                string footprintFilePath = _fileSystem.GetSandboxAppFootprintFilePath();
                var appFootprint = new ApplicationFootprint(footprintFilePath);
                appFootprint.Load(_fileSystem.PackageName);

                // 如果水印发生变化，则说明覆盖安装后首次打开游戏
                if (appFootprint.IsDirty())
                {
                    if (_fileSystem.InstallCleanupMode == EInstallCleanupMode.None)
                    {
                        YooLogger.LogWarning("No action required on overwrite installation.");
                    }
                    else if (_fileSystem.InstallCleanupMode == EInstallCleanupMode.ClearAllCacheFiles)
                    {
                        _fileSystem.DeleteAllBundleFiles();
                        _fileSystem.DeleteAllManifestFiles();
                        _fileSystem.DeleteAllTempFiles();
                        YooLogger.LogWarning("Deleted all cache files on overwrite installation.");
                    }
                    else if (_fileSystem.InstallCleanupMode == EInstallCleanupMode.ClearAllBundleFiles)
                    {
                        _fileSystem.DeleteAllBundleFiles();
                        YooLogger.LogWarning("Deleted all bundle files on overwrite installation.");
                    }
                    else if (_fileSystem.InstallCleanupMode == EInstallCleanupMode.ClearAllManifestFiles)
                    {
                        _fileSystem.DeleteAllManifestFiles();
                        YooLogger.LogWarning("Deleted all manifest files on overwrite installation.");
                    }
                    else
                    {
                        throw new YooInternalException($"Unhandled {nameof(EInstallCleanupMode)} value: {_fileSystem.InstallCleanupMode}.");
                    }

                    appFootprint.Overwrite(_fileSystem.PackageName);
                }

                _steps = ESteps.CopyPackageManifest;
            }

            if (_steps == ESteps.CopyPackageManifest)
            {
                if (_fileSystem.CopyBuiltinPackageManifest)
                {
                    if (_copyBuiltinPackageManifestOp == null)
                    {
                        _copyBuiltinPackageManifestOp = new CopyBuiltinPackageManifestOperation(_fileSystem);
                        _copyBuiltinPackageManifestOp.StartOperation();
                        AddChildOperation(_copyBuiltinPackageManifestOp);
                    }

                    _copyBuiltinPackageManifestOp.UpdateOperation();
                    if (_copyBuiltinPackageManifestOp.IsDone == false)
                        return;

                    if (_copyBuiltinPackageManifestOp.Status == EOperationStatus.Succeeded)
                    {
                        _steps = ESteps.InitializeBuiltinBundleCache;
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetError(_copyBuiltinPackageManifestOp.Error);
                    }
                }
                else
                {
                    _steps = ESteps.InitializeBuiltinBundleCache;
                }
            }

            if (_steps == ESteps.InitializeBuiltinBundleCache)
            {
                if (_initializeBuiltinBundleCacheOp == null)
                {
                    _initializeBuiltinBundleCacheOp = _fileSystem.BuiltinBundleCache.InitializeAsync();
                    _initializeBuiltinBundleCacheOp.StartOperation();
                    AddChildOperation(_initializeBuiltinBundleCacheOp);
                }

                _initializeBuiltinBundleCacheOp.UpdateOperation();
                Progress = _initializeBuiltinBundleCacheOp.Progress;
                if (_initializeBuiltinBundleCacheOp.IsDone == false)
                    return;

                if (_initializeBuiltinBundleCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.InitializeUnpackBundleCache;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_initializeBuiltinBundleCacheOp.Error);
                }
            }

            if (_steps == ESteps.InitializeUnpackBundleCache)
            {
                if (_initializeUnpackBundleCacheOp == null)
                {
                    _initializeUnpackBundleCacheOp = _fileSystem.UnpackBundleCache.InitializeAsync();
                    _initializeUnpackBundleCacheOp.StartOperation();
                    AddChildOperation(_initializeUnpackBundleCacheOp);
                }

                _initializeUnpackBundleCacheOp.UpdateOperation();
                Progress = _initializeUnpackBundleCacheOp.Progress;
                if (_initializeUnpackBundleCacheOp.IsDone == false)
                    return;

                if (_initializeUnpackBundleCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CreateScheduler;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_initializeUnpackBundleCacheOp.Error);
                }
            }

            if (_steps == ESteps.CreateScheduler)
            {
                // 注意: 下载调度中心在最后一步创建，防止初始化失败后残留任务。
                // 注意: 下载调度中心作为独立任务运行！
                if (_fileSystem.UnpackScheduler == null)
                {
                    var schedulerConfig = new DownloadSchedulerOperation.Configuration(
                        schedulerName: _fileSystem.GetType().Name,
                        downloadBackend: _fileSystem.DownloadBackend,
                        maxConcurrency: _fileSystem.UnpackMaxConcurrency,
                        maxRequestsPerFrame: _fileSystem.UnpackMaxRequestsPerFrame);
                    _fileSystem.UnpackScheduler = new DownloadSchedulerOperation(schedulerConfig);
                    AsyncOperationSystem.StartOperation(_fileSystem.PackageName, _fileSystem.UnpackScheduler);
                }

                _steps = ESteps.Done;
                SetResult();
            }
        }
    }
}