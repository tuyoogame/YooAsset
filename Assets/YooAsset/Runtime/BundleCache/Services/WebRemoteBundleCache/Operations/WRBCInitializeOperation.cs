
namespace YooAsset
{
    /// <summary>
    /// Web远端文件缓存初始化操作
    /// </summary>
    internal sealed class WRBCInitializeOperation : BCInitializeOperation
    {
        private readonly WebRemoteBundleCache _fileCache;

        /// <summary>
        /// 创建 Web 远端缓存初始化操作实例
        /// </summary>
        /// <param name="fileCache">Web 远端文件缓存系统</param>
        public WRBCInitializeOperation(WebRemoteBundleCache fileCache)
        {
            _fileCache = fileCache;
        }
        protected override void InternalStart()
        {
            SetResult();
        }
        protected override void InternalUpdate()
        {
        }
    }
}
