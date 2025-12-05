using System.Collections.Generic;

namespace YooAsset
{
    public sealed class SubAssetsHandle : HandleBase
    {
        private System.Action<SubAssetsHandle> _callback;

        internal SubAssetsHandle(ProviderOperation provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<SubAssetsHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(SubAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new YooHandleException($"{nameof(SubAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
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
        public IReadOnlyList<UnityEngine.Object> SubAssetObjects
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.SubAssetObjects;
            }
        }

        /// <summary>
        /// 获取子资源对象
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        /// <param name="assetName">子资源对象名称</param>
        public TObject GetSubAssetObject<TObject>(string assetName) where TObject : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;

            foreach (var assetObject in Provider.SubAssetObjects)
            {
                if (assetObject.name == assetName && assetObject is TObject)
                    return assetObject as TObject;
            }

            YooLogger.Warning($"Not found sub asset object : {assetName}");
            return null;
        }

        /// <summary>
        /// 获取所有的子资源对象集合
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        public TObject[] GetSubAssetObjects<TObject>() where TObject : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;

            List<TObject> result = new List<TObject>(Provider.SubAssetObjects.Length);
            foreach (var assetObject in Provider.SubAssetObjects)
            {
                var retObject = assetObject as TObject;
                if (retObject != null)
                    result.Add(retObject);
            }
            return result.ToArray();
        }
    }
}