
namespace YooAsset
{
    /// <summary>
    /// 下载重试控制器
    /// </summary>
    /// <remarks>
    /// 管理重试次数和重试前的等待计时，不执行实际下载。
    /// </remarks>
    internal sealed class DownloadRetryController
    {
        private readonly int _maxRetryCount;
        private readonly IDownloadRetryPolicy _retryPolicy;
        private double _retryStartTime;
        private float _retryDelay;
        private int _retryCount;

        /// <summary>
        /// 已执行的重试次数
        /// </summary>
        public int RetryCount => _retryCount;

        /// <summary>
        /// 重试等待时长（秒）
        /// </summary>
        public float RetryDelay => _retryDelay;

        /// <summary>
        /// 创建下载重试控制器
        /// </summary>
        /// <param name="maxRetryCount">最大重试次数</param>
        /// <param name="retryPolicy">重试策略</param>
        public DownloadRetryController(int maxRetryCount, IDownloadRetryPolicy retryPolicy)
        {
            _maxRetryCount = maxRetryCount;
            _retryPolicy = retryPolicy;
            _retryStartTime = 0;
            _retryDelay = 0f;
            _retryCount = 0;
        }

        /// <summary>
        /// 判断本次失败是否允许重试
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="httpCode">HTTP 状态码</param>
        /// <param name="httpError">HTTP 错误信息</param>
        /// <returns>允许重试返回 true，否则返回 false。</returns>
        public bool CanRetryRequest(string url, long httpCode, string httpError)
        {
            if (_retryCount >= _maxRetryCount)
                return false;

            if (_retryPolicy.IsRetryableError(url, httpCode, httpError) == false)
                return false;

            YooLogger.LogWarning($"Download request failed. URL: '{url}', HttpCode={httpCode}.");
            return true;
        }

        /// <summary>
        /// 判断当前是否还有可用的重试次数
        /// </summary>
        /// <returns>尚未达到最大重试次数返回 true，否则返回 false。</returns>
        public bool HasRetriesRemaining()
        {
            if (_retryCount >= _maxRetryCount)
                return false;

            return true;
        }

        /// <summary>
        /// 开始本轮重试前的等待
        /// </summary>
        public void StartRetryDelay()
        {
            _retryCount++;
            _retryStartTime = TimeUtility.RealtimeSinceStartup;
            _retryDelay = _retryPolicy.CalculateRetryDelay(_retryCount, _retryDelay);
            YooLogger.LogWarning($"Retrying download in {RetryDelay:F1}s.");
        }

        /// <summary>
        /// 每帧推进重试等待计时
        /// </summary>
        /// <returns>等待时间已到返回 true，否则返回 false。</returns>
        public bool TickRetryDelay()
        {
            return TimeUtility.RealtimeSinceStartup - _retryStartTime >= _retryDelay;
        }
    }
}
