using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 资源包加载与预下载之间的冲突协调器
    /// </summary>
    /// <remarks>
    /// 说明：该协调器按 AssetBundle 资源包隔离冲突：不同资源包可以并发访问，同一资源包的加载与预下载互斥，且加载优先于预下载。
    /// </remarks>
    internal sealed class PreloadConflictController
    {
        /// <summary>
        /// 访问类型
        /// </summary>
        internal enum EAccessType
        {
            AssetBundleLoad,
            CacheDownload,
        }

        /// <summary>
        /// 访问状态
        /// </summary>
        private sealed class AccessState
        {
            public int RequestCount;
            public int ActiveLoadCount;
            public int ActiveDownloadCount;
            public int WaitingLoadCount;
        }

        /// <summary>
        /// 资源包访问请求
        /// </summary>
        internal sealed class AccessRequest : IDisposable
        {
            private readonly PreloadConflictController _controller;
            private readonly string _bundleGuid;
            private readonly EAccessType _accessType;
            private bool _released;
            private bool _acquired;

            public AccessRequest(PreloadConflictController controller, string bundleGuid, EAccessType accessType)
            {
                _controller = controller;
                _bundleGuid = bundleGuid;
                _accessType = accessType;
                _controller.Register(bundleGuid, accessType);
            }

            /// <summary>
            /// 尝试获取访问权限，失败应在后续帧继续重试。
            /// </summary>
            public bool TryAcquire()
            {
                if (_released)
                    throw new YooInternalException($"{nameof(AccessRequest)} has been released.");
                if (_acquired)
                    return true;

                _acquired = _controller.TryAcquire(_bundleGuid, _accessType);
                return _acquired;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_released)
                    return;

                _controller.Release(_bundleGuid, _accessType, _acquired);
                _released = true;
            }
        }


        private readonly bool _enabled;
        private readonly Dictionary<string, AccessState> _accessStates = new Dictionary<string, AccessState>();

        /// <summary>
        /// 是否启用冲突协调
        /// </summary>
        public bool IsEnabled => _enabled;

        public PreloadConflictController(bool enabled)
        {
            _enabled = enabled;
        }

        /// <summary>
        /// 创建资源包访问请求
        /// </summary>
        /// <param name="bundle">目标资源包</param>
        /// <param name="accessType">资源包访问类型</param>
        public AccessRequest CreateAccessRequest(PackageBundle bundle, EAccessType accessType)
        {
            return new AccessRequest(this, bundle.BundleGuid, accessType);
        }

        private void Register(string bundleGuid, EAccessType accessType)
        {
            if (_enabled == false)
                return;

            if (_accessStates.TryGetValue(bundleGuid, out AccessState state) == false)
            {
                state = new AccessState();
                _accessStates.Add(bundleGuid, state);
            }

            state.RequestCount++;

            // 注意：加载在申请时即登记为“等待中”，用于阻止后续预下载抢先。
            if (accessType == EAccessType.AssetBundleLoad)
                state.WaitingLoadCount++;
        }
        private void Release(string bundleGuid, EAccessType accessType, bool acquired)
        {
            if (_enabled == false)
                return;

            AccessState state = GetAccessState(bundleGuid);
            if (acquired)
            {
                if (accessType == EAccessType.AssetBundleLoad)
                    state.ActiveLoadCount--;
                else
                    state.ActiveDownloadCount--;
            }
            else if (accessType == EAccessType.AssetBundleLoad)
            {
                // 尚未通行的加载被取消，撤销等待登记。
                state.WaitingLoadCount--;
            }

            state.RequestCount--;
            if (state.RequestCount == 0)
                _accessStates.Remove(bundleGuid);
        }
        private bool TryAcquire(string bundleGuid, EAccessType accessType)
        {
            if (_enabled == false)
                return true;

            AccessState state = GetAccessState(bundleGuid);
            if (accessType == EAccessType.AssetBundleLoad)
            {
                if (state.ActiveDownloadCount > 0)
                    return false;

                state.WaitingLoadCount--;
                state.ActiveLoadCount++;
                return true;
            }

            // 预下载：任何运行中或等待中的加载都会阻塞它。
            if (state.ActiveLoadCount > 0 || state.WaitingLoadCount > 0)
                return false;

            state.ActiveDownloadCount++;
            return true;
        }
        private AccessState GetAccessState(string bundleGuid)
        {
            if (_accessStates.TryGetValue(bundleGuid, out AccessState state) == false)
                throw new YooInternalException($"Bundle access state not found: '{bundleGuid}'.");
            return state;
        }
    }
}
