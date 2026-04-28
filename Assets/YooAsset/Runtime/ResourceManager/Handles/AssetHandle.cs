using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 资源句柄，用于管理单个资源对象的加载和访问。
    /// </summary>
    public sealed partial class AssetHandle : HandleBase
    {
        private System.Action<AssetHandle> _callback;

        internal AssetHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 当加载完成时触发
        /// </summary>
        public event System.Action<AssetHandle> Completed
        {
            add
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(AssetHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(AssetHandle)} is invalid. It may have been released or the provider was destroyed.");
                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (CheckValidWithWarning() == false)
                return;
            Provider.WaitForCompletion();
        }


        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object AssetObject
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return null;
                return Provider.AssetObject;
            }
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <typeparam name="TAsset">资源类型</typeparam>
        /// <returns>资源对象，如果句柄无效则返回 null。</returns>
        public TAsset GetAssetObject<TAsset>() where TAsset : UnityEngine.Object
        {
            if (CheckValidWithWarning() == false)
                return null;
            return Provider.AssetObject as TAsset;
        }

        /// <summary>
        /// 同步实例化游戏对象
        /// </summary>
        /// <returns>实例化的游戏对象</returns>
        public GameObject InstantiateSync()
        {
            var options = new InstantiateOptions(true);
            return InstantiateSyncInternal(options);
        }

        /// <summary>
        /// 同步实例化游戏对象
        /// </summary>
        /// <param name="options">实例化选项</param>
        /// <returns>实例化的游戏对象</returns>
        public GameObject InstantiateSync(InstantiateOptions options)
        {
            return InstantiateSyncInternal(options);
        }

        /// <summary>
        /// 实例化游戏对象
        /// </summary>
        /// <returns>实例化操作</returns>
        public InstantiateOperation InstantiateAsync()
        {
            var options = new InstantiateOptions(true);
            return InstantiateAsyncInternal(options);
        }

        /// <summary>
        /// 实例化游戏对象
        /// </summary>
        /// <param name="options">实例化选项</param>
        /// <returns>实例化操作</returns>
        public InstantiateOperation InstantiateAsync(InstantiateOptions options)
        {
            return InstantiateAsyncInternal(options);
        }

        private GameObject InstantiateSyncInternal(InstantiateOptions options)
        {
            if (CheckValidWithWarning() == false)
                return null;
            if (Provider.AssetObject == null)
                return null;

            return InstantiateOperation.InstantiateInternal(Provider.AssetObject, options);
        }
        private InstantiateOperation InstantiateAsyncInternal(InstantiateOptions options)
        {
            string packageName = GetAssetInfo().PackageName;
            InstantiateOperation operation = new InstantiateOperation(this, options);
            AsyncOperationSystem.StartOperation(packageName, operation);
            return operation;
        }
    }
}
