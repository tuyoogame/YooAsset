
namespace YooAsset
{
    /// <summary>
    /// 清理所有文件
    /// </summary>
    public abstract class ClearAllBundleFilesOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 编辑器下模拟模式
    /// </summary>
    internal sealed class EditorSimulateModeClearAllBundleFilesOperation : ClearAllBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearAllBundleFiles,
            Done,
        }

        private readonly EditorSimulateModeImpl _impl;
        private FSClearAllBundleFilesOperation _clearAllBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal EditorSimulateModeClearAllBundleFilesOperation(EditorSimulateModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearAllBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearAllBundleFiles)
            {
                if (_clearAllBundleFilesOp == null)
                {
                    _clearAllBundleFilesOp = _impl.EditorFileSystem.ClearAllBundleFilesAsync();
                }

                Progress = _clearAllBundleFilesOp.Progress;
                if (_clearAllBundleFilesOp.IsDone == false)
                    return;

                if (_clearAllBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearAllBundleFilesOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 离线运行模式
    /// </summary>
    internal sealed class OfflinePlayModeClearAllBundleFilesOperation : ClearAllBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearAllBundleFiles,
            Done,
        }

        private readonly OfflinePlayModeImpl _impl;
        private FSClearAllBundleFilesOperation _clearAllBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal OfflinePlayModeClearAllBundleFilesOperation(OfflinePlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearAllBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearAllBundleFiles)
            {
                if (_clearAllBundleFilesOp == null)
                {
                    _clearAllBundleFilesOp = _impl.BuildinFileSystem.ClearAllBundleFilesAsync();
                }

                Progress = _clearAllBundleFilesOp.Progress;
                if (_clearAllBundleFilesOp.IsDone == false)
                    return;

                if (_clearAllBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearAllBundleFilesOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 联机运行模式
    /// </summary>
    internal sealed class HostPlayModeClearAllBundleFilesOperation : ClearAllBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearBuildinAllBundleFiles,
            ClearDeliveryAllBundleFiles,
            ClearCacheAllBundleFiles,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private FSClearAllBundleFilesOperation _clearBuildinAllBundleFilesOp;
        private FSClearAllBundleFilesOperation _clearDeliveryAllBundleFilesOp;
        private FSClearAllBundleFilesOperation _clearCacheAllBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal HostPlayModeClearAllBundleFilesOperation(HostPlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearBuildinAllBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearBuildinAllBundleFiles)
            {
                if (_clearBuildinAllBundleFilesOp == null)
                {
                    _clearBuildinAllBundleFilesOp = _impl.BuildinFileSystem.ClearAllBundleFilesAsync();
                }

                Progress = _clearBuildinAllBundleFilesOp.Progress;
                if (_clearBuildinAllBundleFilesOp.IsDone == false)
                    return;

                if (_clearBuildinAllBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearDeliveryAllBundleFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearBuildinAllBundleFilesOp.Error;
                }
            }

            if (_steps == ESteps.ClearDeliveryAllBundleFiles)
            {
                if (_impl.DeliveryFileSystem == null)
                {
                    _steps = ESteps.ClearCacheAllBundleFiles;
                    return;
                }

                if (_clearDeliveryAllBundleFilesOp == null)
                {
                    _clearDeliveryAllBundleFilesOp = _impl.DeliveryFileSystem.ClearAllBundleFilesAsync();
                }

                Progress = _clearDeliveryAllBundleFilesOp.Progress;
                if (_clearDeliveryAllBundleFilesOp.IsDone == false)
                    return;

                if (_clearDeliveryAllBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.ClearCacheAllBundleFiles;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearDeliveryAllBundleFilesOp.Error;
                }
            }

            if (_steps == ESteps.ClearCacheAllBundleFiles)
            {
                if (_clearCacheAllBundleFilesOp == null)
                {
                    _clearCacheAllBundleFilesOp = _impl.CacheFileSystem.ClearAllBundleFilesAsync();
                }

                Progress = _clearCacheAllBundleFilesOp.Progress;
                if (_clearCacheAllBundleFilesOp.IsDone == false)
                    return;

                if (_clearCacheAllBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearCacheAllBundleFilesOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL运行模式
    /// </summary>
    internal sealed class WebPlayModeClearAllBundleFilesOperation : ClearAllBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            ClearAllBundleFiles,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private FSClearAllBundleFilesOperation _clearAllBundleFilesOp;
        private ESteps _steps = ESteps.None;

        internal WebPlayModeClearAllBundleFilesOperation(WebPlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.ClearAllBundleFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.ClearAllBundleFiles)
            {
                if (_clearAllBundleFilesOp == null)
                {
                    _clearAllBundleFilesOp = _impl.WebFileSystem.ClearAllBundleFilesAsync();
                }

                Progress = _clearAllBundleFilesOp.Progress;
                if (_clearAllBundleFilesOp.IsDone == false)
                    return;

                if (_clearAllBundleFilesOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _clearAllBundleFilesOp.Error;
                }
            }
        }
    }
}