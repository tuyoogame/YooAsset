
namespace YooAsset
{
    internal abstract class DownloadAndCacheFileOperation : AsyncOperationBase
    {
        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public readonly string URL;

        /// <summary>
        /// 下载进度
        /// </summary>
        public float DownloadProgress { get; protected set; }

        /// <summary>
        /// 下载字节
        /// </summary>
        public long DownloadedBytes { get; protected set; }

        public DownloadAndCacheFileOperation(string url)
        {
            URL = url;
        }
        internal override string InternalGetDesc()
        {
            return $"RefCount : {RefCount}";
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }
    }
}