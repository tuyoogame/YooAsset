using UnityEngine;

namespace YooAsset
{
    public sealed class AssetHandle : HandleBase
    {
        private System.Action<AssetHandle> _callback;

        internal AssetHandle(ProviderOperation provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<AssetHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(AssetHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(AssetHandle)} is invalid. It may have been released or the provider was destroyed.");
                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
                return;
            Provider.WaitForAsyncComplete();
        }


        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object AssetObject
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.AssetObject;
            }
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <typeparam name="TAsset">资源类型</typeparam>
        public TAsset GetAssetObject<TAsset>() where TAsset : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;
            return Provider.AssetObject as TAsset;
        }

        /// <summary>
        /// 同步初始化游戏对象
        /// </summary>
        public GameObject InstantiateSync()
        {
            return InstantiateSyncInternal(false, Vector3.zero, Quaternion.identity, null, false);
        }
        public GameObject InstantiateSync(Transform parent)
        {
            return InstantiateSyncInternal(false, Vector3.zero, Quaternion.identity, parent, false);
        }
        public GameObject InstantiateSync(Transform parent, bool worldPositionStays)
        {
            return InstantiateSyncInternal(false, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
        }
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation)
        {
            return InstantiateSyncInternal(true, position, rotation, null, false);
        }
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent)
        {
            return InstantiateSyncInternal(true, position, rotation, parent, false);
        }

        /// <summary>
        /// 异步初始化游戏对象
        /// </summary>
        public InstantiateOperation InstantiateAsync(bool actived = true)
        {
            return InstantiateAsyncInternal(false, Vector3.zero, Quaternion.identity, null, false, actived);
        }
        public InstantiateOperation InstantiateAsync(Transform parent, bool actived = true)
        {
            return InstantiateAsyncInternal(false, Vector3.zero, Quaternion.identity, parent, false, actived);
        }
        public InstantiateOperation InstantiateAsync(Transform parent, bool worldPositionStays, bool actived = true)
        {
            return InstantiateAsyncInternal(false, Vector3.zero, Quaternion.identity, parent, worldPositionStays, actived);
        }
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, bool actived = true)
        {
            return InstantiateAsyncInternal(true, position, rotation, null, false, actived);
        }
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent, bool actived = true)
        {
            return InstantiateAsyncInternal(true, position, rotation, parent, false, actived);
        }

        private GameObject InstantiateSyncInternal(bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            if (IsValidWithWarning == false)
                return null;
            if (Provider.AssetObject == null)
                return null;

            return InstantiateOperation.InstantiateInternal(Provider.AssetObject, setPositionAndRotation, position, rotation, parent, worldPositionStays);
        }
        private InstantiateOperation InstantiateAsyncInternal(bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays, bool actived)
        {
            string packageName = GetAssetInfo().PackageName;
            InstantiateOperation operation = new InstantiateOperation(this, setPositionAndRotation, position, rotation, parent, worldPositionStays, actived);
            OperationSystem.StartOperation(packageName, operation);
            return operation;
        }
    }
}
