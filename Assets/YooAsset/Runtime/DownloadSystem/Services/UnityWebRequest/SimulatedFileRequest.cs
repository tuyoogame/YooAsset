using System;

namespace YooAsset
{
    /// <summary>
    /// 模拟下载器
    /// </summary>
    /// <remarks>
    /// <para>用于编辑器模式下模拟下载进度，不进行实际网络请求。</para>
    /// <para>根据配置的下载速度模拟进度变化</para>
    /// </remarks>
    internal sealed class SimulatedFileRequest : IDownloadFileRequest
    {
        private bool _isDisposed;

        /// <summary>
        /// 模拟下载参数
        /// </summary>
        private readonly SimulatedDownloadRequestArgs _args;

        /// <summary>
        /// 最近一次更新的时间戳（用于计算时间增量）
        /// </summary>
        private double _lastUpdateTime;

        /// <summary>
        /// 小数字节总量累积
        /// </summary>
        private double _bytesAccumulator;

        /// <summary>
        /// 文件保存路径
        /// </summary>
        /// <remarks>
        /// 模拟下载始终返回 null
        /// </remarks>
        public string SavePath
        {
            get { return null; }
        }

        #region 接口实现
        /// <inheritdoc />
        public string Url { get; }

        /// <inheritdoc />
        public bool IsDone
        {
            get
            {
                return Status == EDownloadRequestStatus.Succeeded
                    || Status == EDownloadRequestStatus.Failed;
            }
        }

        /// <inheritdoc />
        public EDownloadRequestStatus Status { get; private set; }

        /// <inheritdoc />
        public float DownloadProgress { get; private set; }

        /// <inheritdoc />
        public long DownloadedBytes { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// 模拟请求成功后返回 200，其他情况下返回 0。
        /// </remarks>
        public long HttpCode { get; private set; }

        /// <inheritdoc />
        public string HttpError { get; private set; }

        /// <inheritdoc />
        public string Error { get; private set; }
        #endregion

        /// <summary>
        /// 构造模拟下载器
        /// </summary>
        /// <param name="args">模拟下载参数</param>
        public SimulatedFileRequest(SimulatedDownloadRequestArgs args)
        {
            _args = args;
            Url = args.Url;
            Status = EDownloadRequestStatus.None;
        }

        /// <inheritdoc />
        public void SendRequest()
        {
            if (_isDisposed)
                return;

            if (Status == EDownloadRequestStatus.None)
            {
                if (_args.DownloadSpeed <= 0)
                {
                    Status = EDownloadRequestStatus.Failed;
                    Error = $"[{GetType().Name}] Download speed must be greater than zero.";
                    return;
                }

                Status = EDownloadRequestStatus.Running;
                _lastUpdateTime = TimeUtility.RealtimeSinceStartup;
            }
        }

        /// <inheritdoc />
        public void UpdateRequest()
        {
            if (_isDisposed)
                return;

            if (Status != EDownloadRequestStatus.Running)
                return;

            double currentTime = TimeUtility.RealtimeSinceStartup;
            double deltaTime = currentTime - _lastUpdateTime;
            _lastUpdateTime = currentTime;

            // 注意：累积小数字节后取整，避免低速高帧率时每帧截断为 0
            _bytesAccumulator += _args.DownloadSpeed * deltaTime;
            DownloadedBytes = (long)_bytesAccumulator;

            if (_args.FileSize > 0)
                DownloadProgress = (float)DownloadedBytes / _args.FileSize;

            // 检查是否完成
            if (DownloadedBytes >= _args.FileSize)
            {
                HttpCode = 200;
                DownloadProgress = 1f;
                DownloadedBytes = _args.FileSize;
                Status = EDownloadRequestStatus.Succeeded;
            }
        }

        /// <inheritdoc />
        public void AbortRequest()
        {
            if (_isDisposed)
                return;

            if (Status == EDownloadRequestStatus.None || Status == EDownloadRequestStatus.Running)
            {
                Status = EDownloadRequestStatus.Failed;
                Error = $"[{GetType().Name}] Request canceled. URL: '{Url}'.";
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}
