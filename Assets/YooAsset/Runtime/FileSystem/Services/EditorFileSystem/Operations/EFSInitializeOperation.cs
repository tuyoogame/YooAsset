
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件系统的初始化操作
    /// </summary>
    internal sealed class EFSInitializeOperation : FSInitializeOperation
    {
        private enum ESteps
        {
            None,
            CheckPlatform,
            InitializeBundleCache,
            CreateScheduler,
            Done,
        }

        private readonly EditorFileSystem _fileSystem;
        private BCInitializeOperation _initializeBundleCacheOp;
        private ESteps _steps = ESteps.None;

        internal EFSInitializeOperation(EditorFileSystem fileSystem)
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
#if !UNITY_EDITOR
                _steps = ESteps.Done;
                SetError($"{nameof(EditorFileSystem)} only supports the Unity Editor.");
#else
                _steps = ESteps.InitializeBundleCache;
#endif
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