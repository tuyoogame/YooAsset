using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    public abstract class UpdatePrePackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 预下载包裹版本
        /// </summary>
        public string PrePackageVersion { protected set; get; }
    }
    /// <summary>
    /// 编辑器下模拟模式的请求远端包裹的预下载版本
    /// </summary>
    internal sealed class EditorPlayModeUpdatePrePackageVersionOperation : UpdatePrePackageVersionOperation
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
    /// 离线模式的请求远端包裹的预下载版本
    /// </summary>
    internal sealed class OfflinePlayModeUpdatePrePackageVersionOperation : UpdatePrePackageVersionOperation
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
    /// 联机模式的请求远端包裹的预下载版本
    /// </summary>
    internal sealed class HostPlayModeUpdatePrePackageVersionOperation : UpdatePrePackageVersionOperation
    {
        private enum ESteps
        {
            None,
            QueryRemotePrePackageVersion,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private QueryRemotePrePackageVersionOperation _queryRemotePrePackageVersionOp;
        private ESteps _steps = ESteps.None;

        internal HostPlayModeUpdatePrePackageVersionOperation(HostPlayModeImpl impl, bool appendTimeTicks, int timeout)
        {
            _impl = impl;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.QueryRemotePrePackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.QueryRemotePrePackageVersion)
            {
                if (_queryRemotePrePackageVersionOp == null)
                {
                    _queryRemotePrePackageVersionOp = new QueryRemotePrePackageVersionOperation(_impl.RemoteServices, _impl.PackageName, _appendTimeTicks, _timeout);
                    OperationSystem.StartOperation(_impl.PackageName, _queryRemotePrePackageVersionOp);
                }

                if (_queryRemotePrePackageVersionOp.IsDone == false)
                    return;

                if (_queryRemotePrePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    PrePackageVersion = _queryRemotePrePackageVersionOp.PrePackageVersion;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _queryRemotePrePackageVersionOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// WebGL模式的请求远端包裹的预下载版本
    /// </summary>
    internal sealed class WebPlayModeUpdatePrePackageVersionOperation : UpdatePrePackageVersionOperation
    {
        private enum ESteps
        {
            None,
            QueryRemotePackageVersion,
            Done,
        }

        private readonly WebPlayModeImpl _impl;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private QueryRemotePrePackageVersionOperation _queryRemotePrePackageVersionOp;
        private ESteps _steps = ESteps.None;

        internal WebPlayModeUpdatePrePackageVersionOperation(WebPlayModeImpl impl, bool appendTimeTicks, int timeout)
        {
            _impl = impl;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.QueryRemotePackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.QueryRemotePackageVersion)
            {
                if (_queryRemotePrePackageVersionOp == null)
                {
                    _queryRemotePrePackageVersionOp = new QueryRemotePrePackageVersionOperation(_impl.RemoteServices, _impl.PackageName, _appendTimeTicks, _timeout);
                    OperationSystem.StartOperation(_impl.PackageName, _queryRemotePrePackageVersionOp);
                }

                if (_queryRemotePrePackageVersionOp.IsDone == false)
                    return;

                if (_queryRemotePrePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    PrePackageVersion = _queryRemotePrePackageVersionOp.PrePackageVersion;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _queryRemotePrePackageVersionOp.Error;
                }
            }
        }
    }
}