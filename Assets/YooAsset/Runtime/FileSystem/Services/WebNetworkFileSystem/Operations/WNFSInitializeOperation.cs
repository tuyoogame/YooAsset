
namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统的初始化操作
    /// </summary>
    internal sealed class WNFSInitializeOperation : FSInitializeOperation
    {
        private enum ESteps
        {
            None,
            CheckPlatform,
            CheckParameter,
            InitializeBundleCache,
            CreateScheduler,
            Done,
        }

        private readonly WebNetworkFileSystem _fileSystem;
        private BCInitializeOperation _initializeBundleCacheOp;
        private ESteps _steps = ESteps.None;

        public WNFSInitializeOperation(WebNetworkFileSystem fileSystem)
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
#if !UNITY_WEBGL
                _steps = ESteps.Done;
                SetError($"{nameof(WebNetworkFileSystem)} only supports the WebGL platform.");
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

                // 检查URL双斜杠
                // 注意：双斜杠会导致某小游戏平台加载文件失败，但网络请求又不返回失败！
                var testUrls = _fileSystem.RemoteService.GetRemoteUrls("test.bundle");
                foreach (var url in testUrls)
                {
                    if (PathUtility.ContainsDoubleSlashes(url))
                    {
                        _steps = ESteps.Done;
                        SetError($"{nameof(IRemoteService)} returned URL contains double slashes: '{url}'.");
                        return;
                    }
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
                // 注意：下载调度中心在最后一步创建，防止初始化失败后残留任务。
                // 注意：下载调度中心作为独立任务运行！
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
