
namespace YooAsset
{
    /// <summary>
    /// 沙盒文件系统的初始化操作
    /// </summary>
    internal sealed class SFSInitializeOperation : FSInitializeOperation
    {
        private enum ESteps
        {
            None,
            CheckPlatform,
            CheckParameter,
            CheckAppFootprint,
            InitializeBundleCache,
            CreateScheduler,
            Done,
        }

        private readonly SandboxFileSystem _fileSystem;
        private BCInitializeOperation _initializeBundleCacheOp;
        private ESteps _steps = ESteps.None;


        internal SFSInitializeOperation(SandboxFileSystem fileSystem)
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
                SetError($"{nameof(SandboxFileSystem)} does not support the WebGL platform.");
#else
                _steps = ESteps.CheckParameter;
#endif
            }

            if (_steps == ESteps.CheckParameter)
            {
                if (_fileSystem.RemoteService == null)
                {
                    _steps = ESteps.Done;
                    SetError($"{nameof(IRemoteService)} is null.");
                    return;
                }

                _steps = ESteps.CheckAppFootprint;
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

                _steps = ESteps.InitializeBundleCache;
            }

            if (_steps == ESteps.InitializeBundleCache)
            {
                if (_initializeBundleCacheOp == null)
                {
                    _initializeBundleCacheOp = _fileSystem.BundleCache.InitializeAsync();
                    _initializeBundleCacheOp.StartOperation();
                    AddChildOperation(_initializeBundleCacheOp);
                }

                _initializeBundleCacheOp.UpdateOperation();
                Progress = _initializeBundleCacheOp.Progress;
                if (_initializeBundleCacheOp.IsDone == false)
                    return;

                if (_initializeBundleCacheOp.Status == EOperationStatus.Succeeded)
                {
                    _steps = ESteps.CreateScheduler;
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_initializeBundleCacheOp.Error);
                }
            }

            if (_steps == ESteps.CreateScheduler)
            {
                // 注意: 下载调度中心在最后一步创建，防止初始化失败后残留任务。
                // 注意: 下载调度中心作为独立任务运行！
                if (_fileSystem.DownloadScheduler == null)
                {
                    var schedulerConfig = new DownloadSchedulerOperation.Configuration(
                        schedulerName: _fileSystem.GetType().Name,
                        downloadBackend: _fileSystem.DownloadBackend,
                        maxConcurrency: _fileSystem.DownloadMaxConcurrency,
                        maxRequestsPerFrame: _fileSystem.DownloadMaxRequestsPerFrame);
                    _fileSystem.DownloadScheduler = new DownloadSchedulerOperation(schedulerConfig);
                    AsyncOperationSystem.StartOperation(_fileSystem.PackageName, _fileSystem.DownloadScheduler);
                }

                _steps = ESteps.Done;
                SetResult();
            }
        }
    }
}