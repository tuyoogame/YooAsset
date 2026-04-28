using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 全资源句柄，用于加载资源包内所有资源对象。
    /// </summary>
    public sealed partial class AllAssetsHandle : HandleBase
    {
        private System.Action<AllAssetsHandle> _callback;

        internal AllAssetsHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 当加载完成时触发
        /// </summary>
        public event System.Action<AllAssetsHandle> Completed
        {
            add
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(AllAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(AllAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
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
        /// 所有资源对象集合
        /// </summary>
        public IReadOnlyList<UnityEngine.Object> AllAssetObjects
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return Array.Empty<UnityEngine.Object>();
                return Provider.AllAssetObjects;
            }
        }
    }
}