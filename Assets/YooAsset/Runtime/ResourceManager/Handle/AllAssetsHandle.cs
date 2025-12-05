using System.Collections.Generic;

namespace YooAsset
{
    public sealed class AllAssetsHandle : HandleBase
    {
        private System.Action<AllAssetsHandle> _callback;

        internal AllAssetsHandle(ProviderOperation provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<AllAssetsHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(AllAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(AllAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
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
        /// 子资源对象集合
        /// </summary>
        public IReadOnlyList<UnityEngine.Object> AllAssetObjects
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.AllAssetObjects;
            }
        }
    }
}