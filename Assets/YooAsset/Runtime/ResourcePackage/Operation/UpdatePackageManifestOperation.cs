
namespace YooAsset
{
    /// <summary>
    /// 向远端请求并更新清单
    /// </summary>
    public abstract class UpdatePackageManifestOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 编辑器下模拟运行
    /// </summary>
    internal sealed class EditorSimulateModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        public EditorSimulateModeUpdatePackageManifestOperation()
        {
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }
    }

    /// <summary>
    /// 离线运行模式
    /// </summary>
    internal sealed class OfflinePlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        public OfflinePlayModeUpdatePackageManifestOperation()
        {
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }
    }

    /// <summary>
    /// 联机运行模式
    /// </summary>
    internal sealed class HostPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        private enum ESteps
        {
            None,
            CheckParams,
            CheckActiveManifest,
            LoadPackageManifest,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageVersion, int timeout)
        {
            _impl = impl;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckParams;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParams)
            {
                if (string.IsNullOrEmpty(_packageVersion))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Package version is null or empty.";
                }
                else
                {
                    _steps = ESteps.CheckActiveManifest;
                }
            }

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象	
                if (_impl.ActiveManifest != null && _impl.ActiveManifest.PackageVersion == _packageVersion)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.LoadPackageManifest;
                }
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadPackageManifestOp == null)
                    _loadPackageManifestOp = _impl.CacheFileSystem.LoadPackageManifestAsync(_packageVersion, _timeout);

                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    _impl.ActiveManifest = _loadPackageManifestOp.Result;            
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadPackageManifestOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL运行模式
    /// </summary>
    internal sealed class WebPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        private enum ESteps
        {
            None,
            CheckParams,
            CheckActiveManifest,
            LoadRemoteManifest,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private ESteps _steps = ESteps.None;


        internal WebPlayModeUpdatePackageManifestOperation(WebPlayModeImpl impl, string packageVersion, int timeout)
        {
            _impl = impl;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckParams;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParams)
            {
                if (string.IsNullOrEmpty(_packageVersion))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Package version is null or empty.";
                }
                else
                {
                    _steps = ESteps.CheckActiveManifest;
                }
            }

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象	
                if (_impl.ActiveManifest != null && _impl.ActiveManifest.PackageVersion == _packageVersion)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.LoadRemoteManifest;
                }
            }

            if (_steps == ESteps.LoadRemoteManifest)
            {
                if (_loadPackageManifestOp == null)
                    _loadPackageManifestOp = _impl.WebFileSystem.LoadPackageManifestAsync(_packageVersion, _timeout);

                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    _impl.ActiveManifest = _loadPackageManifestOp.Result;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadPackageManifestOp.Error;
                }
            }
        }
    }
}