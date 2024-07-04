
namespace YooAsset
{
    /// <summary>
    /// 查询远端包裹的最新版本
    /// </summary>
    public abstract class RequestPackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前最新的包裹版本
        /// </summary>
        public string PackageVersion { protected set; get; }
    }

    /// <summary>
    /// 编辑器下模拟运行
    /// </summary>
    internal class EditorSimulateModeRequestPackageVersionOperation : RequestPackageVersionOperation
    {
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
    internal class OfflinePlayModeRequestPackageVersionOperation : RequestPackageVersionOperation
    {
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
    internal class HostPlayModeRequestPackageVersionOperation : RequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            QueryPackageVersion,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private FSRequestPackageVersionOperation _queryPackageVersionOp;
        private ESteps _steps = ESteps.None;

        internal HostPlayModeRequestPackageVersionOperation(HostPlayModeImpl impl, bool appendTimeTicks, int timeout)
        {
            _impl = impl;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.QueryPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.QueryPackageVersion)
            {
                if (_queryPackageVersionOp == null)
                {
                    _queryPackageVersionOp = _impl.CacheFileSystem.RequestPackageVersionAsync(_appendTimeTicks, _timeout);
                }

                if (_queryPackageVersionOp.IsDone == false)
                    return;

                if (_queryPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = _queryPackageVersionOp.PackageVersion;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _queryPackageVersionOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL运行模式
    /// </summary>
    internal class WebPlayModeRequestPackageVersionOperation : RequestPackageVersionOperation
    {
        private enum ESteps
        {
            None,
            QueryPackageVersion,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private FSRequestPackageVersionOperation _queryPackageVersionOp;
        private ESteps _steps = ESteps.None;

        internal WebPlayModeRequestPackageVersionOperation(WebPlayModeImpl impl, bool appendTimeTicks, int timeout)
        {
            _impl = impl;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.QueryPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.QueryPackageVersion)
            {
                if (_queryPackageVersionOp == null)
                {
                    _queryPackageVersionOp = _impl.WebFileSystem.RequestPackageVersionAsync(_appendTimeTicks, _timeout);
                }

                if (_queryPackageVersionOp.IsDone == false)
                    return;

                if (_queryPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = _queryPackageVersionOp.PackageVersion;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _queryPackageVersionOp.Error;
                }
            }
        }
    }
}