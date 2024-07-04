using UnityEngine;

namespace YooAsset
{
    internal abstract class FSLoadBundleOperation : AsyncOperationBase
    {
        /// <summary>
        /// 加载结果
        /// </summary>
        public object Result { protected set; get; }

        /// <summary>
        /// 下载进度
        /// </summary>
        public float DownloadProgress { protected set; get; } = 0;

        /// <summary>
        /// 下载大小
        /// </summary>
        public long DownloadedBytes { protected set; get; } = 0;

        /// <summary>
        /// 终止下载任务
        /// </summary>
        public abstract void AbortDownloadOperation();
    }
}