
namespace YooAsset
{
    /// <summary>
    /// 原生文件句柄，用于访问未经 Unity 处理的原始文件。
    /// </summary>
    public sealed partial class RawFileHandle : HandleBase
    {
        private System.Action<RawFileHandle> _callback;

        internal RawFileHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 当加载完成时触发
        /// </summary>
        public event System.Action<RawFileHandle> Completed
        {
            add
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(RawFileHandle)} is invalid. It may have been released or the provider was destroyed.");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (CheckValidWithWarning() == false)
                    throw new YooHandleInvalidException($"{nameof(RawFileHandle)} is invalid. It may have been released or the provider was destroyed.");
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
        /// 获取原生文件的路径
        /// </summary>
        /// <returns>原生文件的磁盘路径</returns>
        public string GetRawFilePath()
        {
            if (CheckValidWithWarning() == false)
                return string.Empty;
            return Provider.LoadedBundleHandle.BundleFilePath;
        }
    }
}