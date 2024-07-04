
namespace YooAsset
{
    /// <summary>
    /// 清理未使用的文件
    /// </summary>
    public abstract class ClearUnusedBundleFilesOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 编辑器下模拟模式
    /// </summary>
    internal sealed class EditorSimulateModeClearUnusedBundleFilesOperation : ClearUnusedBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearUnusedBundleFiles,
            Done,
        }

        private readonly EditorSimulateModeImpl _impl;
        private FSClearUnusedBundleFilesOperation _clearUnusedBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal EditorSimulateModeClearUnusedBundleFilesOperation(EditorSimulateModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearUnusedBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearUnusedBundleFiles)
            {
                if (_clearUnusedBundleFilesOp == null)
                {
                    _clearUnusedBundleFilesOp = _impl.EditorFileSystem.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);
                }

                Progress = _clearUnusedBundleFilesOp.Progress;
                if (_clearUnusedBundleFilesOp.IsDone == false)
                    return;

                if (_clearUnusedBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearUnusedBundleFilesOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 离线运行模式
    /// </summary>
    internal sealed class OfflinePlayModeClearUnusedBundleFilesOperation : ClearUnusedBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearUnusedBundleFiles,
            Done,
        }

        private readonly OfflinePlayModeImpl _impl;
        private FSClearUnusedBundleFilesOperation _clearUnusedBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal OfflinePlayModeClearUnusedBundleFilesOperation(OfflinePlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearUnusedBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearUnusedBundleFiles)
            {
                if (_clearUnusedBundleFilesOp == null)
                {
                    _clearUnusedBundleFilesOp = _impl.BuildinFileSystem.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);
                }

                Progress = _clearUnusedBundleFilesOp.Progress;
                if (_clearUnusedBundleFilesOp.IsDone == false)
                    return;

                if (_clearUnusedBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearUnusedBundleFilesOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 联机运行模式
    /// </summary>
    internal sealed class HostPlayModeClearUnusedBundleFilesOperation : ClearUnusedBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearBuildinUnusedBundleFiles,
            ClearDeliveryUnusedBundleFiles,
            ClearCacheUnusedBundleFiles,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private FSClearUnusedBundleFilesOperation _clearBuildinUnusedBundleFilesOp;
        private FSClearUnusedBundleFilesOperation _clearDeliveryUnusedBundleFilesOp;
        private FSClearUnusedBundleFilesOperation _clearCacheUnusedBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal HostPlayModeClearUnusedBundleFilesOperation(HostPlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearBuildinUnusedBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearBuildinUnusedBundleFiles)
            {
                if (_clearBuildinUnusedBundleFilesOp == null)
                {
                    _clearBuildinUnusedBundleFilesOp = _impl.BuildinFileSystem.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);
                }

                Progress = _clearBuildinUnusedBundleFilesOp.Progress;
                if (_clearBuildinUnusedBundleFilesOp.IsDone == false)
                    return;

                if (_clearBuildinUnusedBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearDeliveryUnusedBundleFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearBuildinUnusedBundleFilesOp.Error;
                }
            }

            if (_steps == ESteps.ClearDeliveryUnusedBundleFiles)
            {
                if (_impl.DeliveryFileSystem == null)
                {
                    _steps = ESteps.ClearCacheUnusedBundleFiles;
                    return;
                }

                if (_clearDeliveryUnusedBundleFilesOp == null)
                {
                    _clearDeliveryUnusedBundleFilesOp = _impl.DeliveryFileSystem.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);
                }

                Progress = _clearDeliveryUnusedBundleFilesOp.Progress;
                if (_clearDeliveryUnusedBundleFilesOp.IsDone == false)
                    return;

                if (_clearDeliveryUnusedBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearCacheUnusedBundleFiles;

                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearDeliveryUnusedBundleFilesOp.Error;
                }
            }

            if (_steps == ESteps.ClearCacheUnusedBundleFiles)
            {
                if (_clearCacheUnusedBundleFilesOp == null)
                {
                    _clearCacheUnusedBundleFilesOp = _impl.CacheFileSystem.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);
                }

                Progress = _clearCacheUnusedBundleFilesOp.Progress;
                if (_clearCacheUnusedBundleFilesOp.IsDone == false)
                    return;

                if (_clearCacheUnusedBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearCacheUnusedBundleFilesOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL运行模式
    /// </summary>
    internal sealed class WebPlayModeClearUnusedBundleFilesOperation : ClearUnusedBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearUnusedBundleFiles,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private FSClearUnusedBundleFilesOperation _clearUnusedBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal WebPlayModeClearUnusedBundleFilesOperation(WebPlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearUnusedBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearUnusedBundleFiles)
            {
                if (_clearUnusedBundleFilesOp == null)
                {
                    _clearUnusedBundleFilesOp = _impl.WebFileSystem.ClearUnusedBundleFilesAsync(_impl.ActiveManifest);
                }

                Progress = _clearUnusedBundleFilesOp.Progress;
                if (_clearUnusedBundleFilesOp.IsDone == false)
                    return;

                if (_clearUnusedBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearUnusedBundleFilesOp.Error;
                }
            }
        }
    }
}
