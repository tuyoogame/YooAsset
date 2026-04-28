using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 子资源句柄，用于管理资源包内子资源对象的加载和访问。
    /// </summary>
    public sealed partial class SubAssetsHandle : HandleBase
    {
        private System.Action<SubAssetsHandle> _callback;

        internal SubAssetsHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 当加载完成时触发
        /// </summary>
        public event System.Action<SubAssetsHandle> Completed
        {
            add
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(SubAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(SubAssetsHandle)} is invalid. It may have been released or the provider was destroyed.");
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
        /// 子资源对象集合
        /// </summary>
        public IReadOnlyList<UnityEngine.Object> SubAssetObjects
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return Array.Empty<UnityEngine.Object>();
                return Provider.SubAssetObjects;
            }
        }

        /// <summary>
        /// 获取子资源对象
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        /// <param name="assetName">子资源对象名称</param>
        /// <returns>匹配的子资源对象，未找到则返回 null。</returns>
        public TObject GetSubAssetObject<TObject>(string assetName) where TObject : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(assetName));
            if (CheckValidWithWarning() == false)
                return null;

            foreach (var assetObject in Provider.SubAssetObjects)
            {
                if (assetObject.name == assetName && assetObject is TObject)
                    return assetObject as TObject;
            }

            YooLogger.LogWarning($"Sub asset object not found: '{assetName}'.");
            return null;
        }

        /// <summary>
        /// 获取所有的子资源对象集合
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        /// <returns>匹配类型的子资源对象集合</returns>
        public IReadOnlyList<TObject> GetSubAssetObjects<TObject>() where TObject : UnityEngine.Object
        {
            if (CheckValidWithWarning() == false)
                return Array.Empty<TObject>();

            List<TObject> result = new List<TObject>(Provider.SubAssetObjects.Length);
            foreach (var assetObject in Provider.SubAssetObjects)
            {
                var retObject = assetObject as TObject;
                if (retObject != null)
                    result.Add(retObject);
            }
            return result;
        }
    }
}