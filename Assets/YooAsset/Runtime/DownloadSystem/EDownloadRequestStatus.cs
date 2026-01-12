
namespace YooAsset
{
    /// <summary>
    /// 下载请求状态
    /// </summary>
    internal enum EDownloadRequestStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None,

        /// <summary>
        /// 进行中
        /// </summary>
        Running,

        /// <summary>
        /// 中止中
        /// </summary>
        Aborting,

        /// <summary>
        /// 已成功
        /// </summary>
        Succeeded,

        /// <summary>
        /// 已失败
        /// </summary>
        Failed,
    }
}
