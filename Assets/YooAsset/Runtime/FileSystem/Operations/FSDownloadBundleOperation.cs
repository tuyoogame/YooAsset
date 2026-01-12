
namespace YooAsset
{
    /// <summary>
    /// 下载资源包操作的抽象基类
    /// </summary>
    internal abstract class FSDownloadBundleOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; private set; }

        /// <summary>
        /// 下载报告
        /// </summary>
        public DownloadReport Report { get; protected set; }

        internal FSDownloadBundleOperation(PackageBundle bundle)
        {
            Bundle = bundle;
        }
    }

    /// <summary>
    /// 下载资源包操作的立即完成实现
    /// </summary>
    internal sealed class FSDownloadBundleCompleteOperation : FSDownloadBundleOperation
    {
        private readonly string _error;

        internal FSDownloadBundleCompleteOperation(string error) : base(null)
        {
            _error = error;
        }
        protected override void InternalStart()
        {
            if (string.IsNullOrEmpty(_error))
            {
                SetResult();
            }
            else
            {
                SetError(_error);
            }
        }
        protected override void InternalUpdate()
        {
        }
    }
}