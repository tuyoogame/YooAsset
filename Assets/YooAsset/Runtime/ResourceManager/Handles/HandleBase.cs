using System;
using System.Collections;

namespace YooAsset
{
    /// <summary>
    /// 资源句柄基类，提供资源加载状态查询和释放功能。
    /// </summary>
    public abstract partial class HandleBase : IEnumerator, IDisposable
    {
        private readonly AssetInfo _assetInfo;

        /// <summary>
        /// 关联的资源提供者
        /// </summary>
        internal ProviderBase Provider { private set; get; }

        internal HandleBase(ProviderBase provider)
        {
            Provider = provider;
            _assetInfo = provider.MainAssetInfo;
        }
        internal abstract void InvokeCallback();

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Release()
        {
            if (CheckValidWithWarning() == false)
                return;
            Provider.ReleaseHandle(this);

            // 主动卸载零引用的资源包
            if (Provider.RefCount == 0)
                Provider.TryUnloadBundle();

            Provider = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Release();
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <returns>资源信息</returns>
        public AssetInfo GetAssetInfo()
        {
            return _assetInfo;
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public EOperationStatus Status
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return EOperationStatus.None;
                return Provider.Status;
            }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return string.Empty;
                return Provider.Error;
            }
        }

        /// <summary>
        /// 加载进度
        /// </summary>
        public float Progress
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return 0;
                return Provider.Progress;
            }
        }

        /// <summary>
        /// 是否加载完毕
        /// </summary>
        public bool IsDone
        {
            get
            {
                if (CheckValidWithWarning() == false)
                    return true;
                return Provider.IsDone;
            }
        }

        /// <summary>
        /// 句柄是否有效
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (Provider != null && Provider.IsDestroyed == false)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 检查句柄是否有效，无效时输出警告日志。
        /// </summary>
        internal bool CheckValidWithWarning()
        {
            if (Provider != null && Provider.IsDestroyed == false)
            {
                return true;
            }
            else
            {
                if (Provider == null)
                    YooLogger.LogWarning($"Handle has already been released. Asset: '{_assetInfo.AssetPath}'.");
                else if (Provider.IsDestroyed)
                    YooLogger.LogWarning($"Provider has already been destroyed. Asset: '{_assetInfo.AssetPath}'.");
                return false;
            }
        }

        #region 异步操作相关
        /// <summary>
        /// 获取用于 async/await 的 Awaiter
        /// </summary>
        /// <returns>用于 async/await 的 Awaiter 对象</returns>
        public OperationAwaiter GetAwaiter()
        {
            if (CheckValidWithWarning() == false)
                throw new InvalidOperationException("Handle is not valid.");
            return Provider.GetAwaiter();
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        /// <inheritdoc/>
        void IEnumerator.Reset()
        {
        }
        /// <inheritdoc/>
        object IEnumerator.Current
        {
            get { return Provider; }
        }
        #endregion
    }
}