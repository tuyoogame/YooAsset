
namespace YooAsset
{
    /// <summary>
    /// Web服务端文件系统的初始化操作
    /// </summary>
    internal sealed class WSFSInitializeOperation : FSInitializeOperation
    {
        private enum ESteps
        {
            None,
            CheckPlatform,
            InitializeBundleCache,
            Done,
        }

        private readonly WebServerFileSystem _fileSystem;
        private BCInitializeOperation _initializeBundleCacheOp;
        private ESteps _steps = ESteps.None;

        public WSFSInitializeOperation(WebServerFileSystem fileSystem)
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
                SetError($"{nameof(WebServerFileSystem)} only supports the WebGL platform.");
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
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_initializeBundleCacheOp.Error);
                }
            }
        }
    }
}