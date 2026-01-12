
namespace YooAsset
{
    /// <summary>
    /// 下载重试策略
    /// </summary>
    public interface IDownloadRetryPolicy
    {
        /// <summary>
        /// 判断本次下载失败是否属于可重试的错误
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="httpCode">HTTP 状态码（0 表示网络中断或非 HTTP 错误）</param>
        /// <param name="httpError">服务器返回的错误描述文本</param>
        /// <returns>true 允许重试；false 应立即失败。</returns>
        bool IsRetryableError(string url, long httpCode, string httpError);

        /// <summary>
        /// 计算本次重试应等待的时长（秒）
        /// </summary>
        /// <param name="retryCount">即将进入的重试次数（从 1 开始）</param>
        /// <param name="previousDelay">上一次等待时长（首次时为 0）</param>
        /// <returns>本次应等待的秒数</returns>
        float CalculateRetryDelay(int retryCount, float previousDelay);
    }
}
