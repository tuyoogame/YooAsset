
namespace YooAsset
{
    /// <summary>
    /// Web 网络文件系统的加载资源包操作
    /// </summary>
    internal sealed class WNFSLoadPackageBundleOperation : FSLoadPackageBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBundle,
            Done,
        }

        private readonly WebNetworkFileSystem _fileSystem;
        private readonly FSLoadPackageBundleOptions _options;
        private BCLoadBundleOperation _loadBundleOp;
        private ESteps _steps = ESteps.None;

        internal WNFSLoadPackageBundleOperation(WebNetworkFileSystem fileSystem, FSLoadPackageBundleOptions options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.LoadBundle;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBundle)
            {
                if (_loadBundleOp == null)
                {
                    _loadBundleOp = _fileSystem.BundleCache.LoadBundleAsync(_options.ConvertTo());
                    _loadBundleOp.StartOperation();
                    AddChildOperation(_loadBundleOp);
                }

                _loadBundleOp.UpdateOperation();
                Progress = _loadBundleOp.Progress;
                if (_loadBundleOp.IsDone == false)
                    return;

                if (_loadBundleOp.Status == EOperationStatus.Succeeded)
                {
                    if (_loadBundleOp.BundleHandle == null)
                    {
                        _steps = ESteps.Done;
                        SetError("Fatal error: loaded bundle handle is null.");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        SetResult();
                        BundleHandle = _loadBundleOp.BundleHandle;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError(_loadBundleOp.Error);
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            if (_steps != ESteps.Done)
            {
                _steps = ESteps.Done;
                SetError("WebGL platform does not support synchronous loading.");
                YooLogger.LogError(Error);
            }
        }
    }
}
