
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
