
namespace YooAsset
{
    /// <summary>
    /// 文件下载操作的抽象基类
    /// </summary>
    internal abstract class DownloadFileBaseOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount { get; private set; }

        /// <summary>
        /// 最新下载状态报告
        /// </summary>
        public DownloadReport LatestReport { get; internal set; }

        /// <summary>
        /// 构造文件下载操作
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="url">下载地址</param>
        public DownloadFileBaseOperation(PackageBundle packageBundle, string url)
        {
            Bundle = packageBundle;
            Url = url;
        }
        protected override string InternalGetDescription()
        {
            return $"Reference count: {ReferenceCount}";
        }

        /// <summary>
        /// 释放一次引用
        /// </summary>
        /// <remarks>
        /// 当引用计数降为零时，下载调度器可终止并移除该任务。
        /// </remarks>
        public void Release()
        {
            ReferenceCount--;
            if (ReferenceCount < 0)
                throw new YooInternalException($"ReferenceCount is negative for bundle: '{Bundle.BundleGuid}'.");
        }

        /// <summary>
        /// 增加一次引用
        /// </summary>
        /// <remarks>
        /// 当同一资源被多个请求复用时，可通过增加引用计数延长任务生命周期。
        /// </remarks>
        public void Reference()
        {
            ReferenceCount++;
        }
    }
}