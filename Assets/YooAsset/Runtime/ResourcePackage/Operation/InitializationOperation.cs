
namespace YooAsset
{
    /// <summary>
    /// 初始化操作
    /// </summary>
    public abstract class InitializationOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 编辑器下模拟模式
    /// </summary>
    internal sealed class EditorSimulateModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateFileSystem,
            InitFileSystem,
            Done,
        }

        private readonly EditorSimulateModeImpl _impl;
        private readonly EditorSimulateModeParameters _parameters;
        private FSInitializeFileSystemOperation _initFileSystemOp;
        private ESteps _steps = ESteps.None;

        internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, EditorSimulateModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.CreateFileSystem)
            {
                if (_parameters.EditorFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Editor file system parameters is null";
                    return;
                }

                _impl.EditorFileSystem = PlayModeHelper.CreateFileSystem(_impl.PackageName, _parameters.EditorFileSystemParameters);
                if (_impl.EditorFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create editor file system";
                    return;
                }

                _steps = ESteps.InitFileSystem;
            }

            if (_steps == ESteps.InitFileSystem)
            {
                if (_initFileSystemOp == null)
                    _initFileSystemOp = _impl.EditorFileSystem.InitializeFileSystemAsync();

                Progress = _initFileSystemOp.Progress;
                if (_initFileSystemOp.IsDone == false)
                    return;

                if (_initFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initFileSystemOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 离线运行模式
    /// </summary>
    internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateFileSystem,
            InitFileSystem,
            Done,
        }

        private readonly OfflinePlayModeImpl _impl;
        private readonly OfflinePlayModeParameters _parameters;
        private FSInitializeFileSystemOperation _initFileSystemOp;
        private FSRequestPackageVersionOperation _requestPackageVersionOp;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private ESteps _steps = ESteps.None;

        internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, OfflinePlayModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateFileSystem)
            {
                if (_parameters.BuildinFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Buildin file system parameters is null";
                    return;
                }

                _impl.BuildinFileSystem = PlayModeHelper.CreateFileSystem(_impl.PackageName, _parameters.BuildinFileSystemParameters);
                if (_impl.BuildinFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create buildin file system";
                    return;
                }

                _steps = ESteps.InitFileSystem;
            }

            if (_steps == ESteps.InitFileSystem)
            {
                if (_initFileSystemOp == null)
                    _initFileSystemOp = _impl.BuildinFileSystem.InitializeFileSystemAsync();

                Progress = _initFileSystemOp.Progress;
                if (_initFileSystemOp.IsDone == false)
                    return;

                if (_initFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initFileSystemOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 联机运行模式
    /// </summary>
    internal sealed class HostPlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateFileSystem,
            InitBuildinFileSystem,
            InitDeliveryFileSystem,
            InitCacheFileSystem,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly HostPlayModeParameters _parameters;
        private FSInitializeFileSystemOperation _initBuildinFileSystemOp;
        private FSInitializeFileSystemOperation _initDeliveryFileSystemOp;
        private FSInitializeFileSystemOperation _initCacheFileSystemOp;
        private ESteps _steps = ESteps.None;

        internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, HostPlayModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateFileSystem)
            {
                if (_parameters.CacheFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Cache file system parameters is null";
                    return;
                }

                if (_parameters.BuildinFileSystemParameters != null)
                {
                    _impl.BuildinFileSystem = PlayModeHelper.CreateFileSystem(_impl.PackageName, _parameters.BuildinFileSystemParameters);
                    if (_impl.BuildinFileSystem == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Failed to create buildin file system";
                        return;
                    }
                    return;
                }

                if (_parameters.DeliveryFileSystemParameters != null)
                {
                    _impl.DeliveryFileSystem = PlayModeHelper.CreateFileSystem(_impl.PackageName, _parameters.DeliveryFileSystemParameters);
                    if (_impl.DeliveryFileSystem == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Failed to create delivery file system";
                        return;
                    }
                }

                _impl.CacheFileSystem = PlayModeHelper.CreateFileSystem(_impl.PackageName, _parameters.CacheFileSystemParameters);
                if (_impl.CacheFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create cache file system";
                    return;
                }

                _steps = ESteps.InitBuildinFileSystem;
            }

            if (_steps == ESteps.InitBuildinFileSystem)
            {
                // 注意：内置文件系统可以为空
                if (_impl.BuildinFileSystem == null)
                {
                    _steps = ESteps.InitDeliveryFileSystem;
                    return;
                }

                if (_initBuildinFileSystemOp == null)
                    _initBuildinFileSystemOp = _impl.BuildinFileSystem.InitializeFileSystemAsync();

                Progress = _initBuildinFileSystemOp.Progress;
                if (_initBuildinFileSystemOp.IsDone == false)
                    return;

                if (_initBuildinFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.InitDeliveryFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initBuildinFileSystemOp.Error;
                }
            }

            if (_steps == ESteps.InitDeliveryFileSystem)
            {
                // 注意：分发文件系统可以为空
                if (_impl.DeliveryFileSystem == null)
                {
                    _steps = ESteps.InitCacheFileSystem;
                    return;
                }

                Progress = _initDeliveryFileSystemOp.Progress;
                if (_initDeliveryFileSystemOp == null)
                    _initDeliveryFileSystemOp = _impl.DeliveryFileSystem.InitializeFileSystemAsync();

                if (_initDeliveryFileSystemOp.IsDone == false)
                    return;

                if (_initDeliveryFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.InitCacheFileSystem;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initDeliveryFileSystemOp.Error;
                }
            }

            if (_steps == ESteps.InitCacheFileSystem)
            {
                if (_initCacheFileSystemOp == null)
                    _initCacheFileSystemOp = _impl.CacheFileSystem.InitializeFileSystemAsync();

                Progress = _initCacheFileSystemOp.Progress;
                if (_initCacheFileSystemOp.IsDone == false)
                    return;

                if (_initCacheFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initCacheFileSystemOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL运行模式
    /// </summary>
    internal sealed class WebPlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            CreateFileSystem,
            InitWebFileSystem,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private readonly WebPlayModeParameters _parameters;
        private FSInitializeFileSystemOperation _initWebFileSystemOp;
        private ESteps _steps = ESteps.None;

        internal WebPlayModeInitializationOperation(WebPlayModeImpl impl, WebPlayModeParameters parameters)
        {
            _impl = impl;
            _parameters = parameters;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateFileSystem;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateFileSystem)
            {
                if (_parameters.WebFileSystemParameters == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Web file system parameters is null";
                    return;
                }

                _impl.WebFileSystem = PlayModeHelper.CreateFileSystem(_impl.PackageName, _parameters.WebFileSystemParameters);
                if (_impl.WebFileSystem == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to create web file system";
                    return;
                }

                _steps = ESteps.InitWebFileSystem;
            }

            if (_steps == ESteps.InitWebFileSystem)
            {
                if (_initWebFileSystemOp == null)
                    _initWebFileSystemOp = _impl.WebFileSystem.InitializeFileSystemAsync();

                Progress = _initWebFileSystemOp.Progress;
                if (_initWebFileSystemOp.IsDone == false)
                    return;

                if (_initWebFileSystemOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initWebFileSystemOp.Error;
                }
            }
        }
    }
}