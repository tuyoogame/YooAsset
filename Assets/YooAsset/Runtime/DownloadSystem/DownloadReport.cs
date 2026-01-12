
namespace YooAsset
{
    /// <summary>
    /// 下载状态报告
    /// </summary>
    public readonly struct DownloadReport
    {
        /// <summary>
        /// HTTP 响应状态码
        /// </summary>
        public long HttpCode { get; }

        /// <summary>
        /// HTTP 错误信息
        /// </summary>
        public string HttpError { get; }

        /// <summary>
        /// 当前下载的字节数
        /// </summary>
        public long DownloadedBytes { get; }

        /// <summary>
        /// 当前下载进度（0f - 1f）
        /// </summary>
        public float DownloadProgress { get; }

        /// <summary>
        /// 空的下载报告实例（所有字段为默认值）
        /// </summary>
        public static readonly DownloadReport Empty = default;

        /// <summary>
        /// 构造下载状态报告
        /// </summary>
        /// <param name="httpCode">HTTP 响应状态码</param>
        /// <param name="httpError">HTTP 错误信息</param>
        /// <param name="downloadedBytes">已下载的字节数</param>
        /// <param name="downloadProgress">下载进度，取值范围 0f ~ 1f</param>
        public DownloadReport(long httpCode, string httpError, long downloadedBytes, float downloadProgress)
        {
            HttpCode = httpCode;
            HttpError = httpError;
            DownloadedBytes = downloadedBytes;
            DownloadProgress = downloadProgress;
        }

        /// <summary>
        /// 创建下载进度报告
        /// </summary>
        /// <param name="downloadedBytes">已下载的字节数</param>
        /// <param name="downloadProgress">下载进度，取值范围 0f ~ 1f</param>
        /// <returns>不含 HTTP 状态信息的进度报告</returns>
        public static DownloadReport CreateProgress(long downloadedBytes, float downloadProgress)
        {
            return new DownloadReport(
                httpCode: 0,
                httpError: null,
                downloadedBytes: downloadedBytes,
                downloadProgress: downloadProgress);
        }

        /// <summary>
        /// 创建包含 HTTP 状态的最终报告（成功或失败）
        /// </summary>
        /// <param name="httpCode">HTTP 响应状态码</param>
        /// <param name="httpError">HTTP 错误信息，成功时为 null。</param>
        /// <param name="downloadedBytes">已下载的字节数</param>
        /// <param name="downloadProgress">下载进度，取值范围 0f ~ 1f</param>
        /// <returns>包含完整 HTTP 状态信息的最终报告</returns>
        public static DownloadReport CreateFinished(long httpCode, string httpError, long downloadedBytes, float downloadProgress)
        {
            return new DownloadReport(
                httpCode: httpCode,
                httpError: httpError,
                downloadedBytes: downloadedBytes,
                downloadProgress: downloadProgress);
        }
    }
}
