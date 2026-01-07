using System;

namespace YooAsset
{
    /// <summary>
    /// 模拟下载器
    /// </summary>
    /// <remarks>
    /// 用于编辑器模式下模拟下载进度，不进行实际网络请求。
    /// 根据配置的下载速度模拟进度变化。
    /// </remarks>
    internal sealed class VirtualFileDownloader : IDownloadFileRequest
    {
        private readonly DownloadSimulateRequestArgs _args;
        private double _lastUpdateTime;

        /// <summary>
        /// 文件保存路径（模拟下载不需要）
        /// </summary>
        public string SavePath
        {
            get { return null; }
        }

        #region 接口实现
        /// <summary>
        /// 请求地址
        /// </summary>
        public string URL { get; }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                PollingRequest();
                return Status == EDownloadRequestStatus.Succeed
                    || Status == EDownloadRequestStatus.Failed
                    || Status == EDownloadRequestStatus.Aborted;
            }
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public EDownloadRequestStatus Status { get; private set; }

        /// <summary>
        /// 当前下载进度（0f - 1f）
        /// </summary>
        public float DownloadProgress { get; private set; }

        /// <summary>
        /// 当前请求已接收的字节数
        /// </summary>
        public long DownloadedBytes { get; private set; }

        /// <summary>
        /// HTTP 返回码（模拟固定返回 200）
        /// </summary>
        public long HttpCode { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; private set; }
        #endregion

        /// <summary>
        /// 构造模拟下载器
        /// </summary>
        /// <param name="args">模拟下载参数</param>
        public VirtualFileDownloader(DownloadSimulateRequestArgs args)
        {
            _args = args;
            URL = args.URL;
            Status = EDownloadRequestStatus.None;
        }

        /// <summary>
        /// 发起请求
        /// </summary>
        public void SendRequest()
        {
            if (Status == EDownloadRequestStatus.None)
            {
                Status = EDownloadRequestStatus.Running;
                _lastUpdateTime = GetUnityEngineRealtime();
            }
        }

        /// <summary>
        /// 轮询请求
        /// </summary>
        public void PollingRequest()
        {
            if (Status != EDownloadRequestStatus.Running)
                return;

            double currentTime = GetUnityEngineRealtime();
            double deltaTime = currentTime - _lastUpdateTime;
            _lastUpdateTime = currentTime;

            // 计算本帧下载的字节数
            long downloadBytes = (long)(_args.DownloadSpeed * deltaTime);
            DownloadedBytes += downloadBytes;

            if (_args.FileSize > 0)
                DownloadProgress = (float)DownloadedBytes / _args.FileSize;

            // 检查是否完成
            if (DownloadedBytes >= _args.FileSize)
            {
                HttpCode = 200;
                DownloadProgress = 1f;
                DownloadedBytes = _args.FileSize;
                Status = EDownloadRequestStatus.Succeed;
            }
        }

        /// <summary>
        /// 中止请求
        /// </summary>
        public void AbortRequest()
        {
            if (Status == EDownloadRequestStatus.None || Status == EDownloadRequestStatus.Running)
            {
                Status = EDownloadRequestStatus.Aborted;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
        }

        private double GetUnityEngineRealtime()
        {
#if UNITY_2020_3_OR_NEWER
            return UnityEngine.Time.realtimeSinceStartupAsDouble;
#else
            return UnityEngine.Time.realtimeSinceStartup;
#endif
        }
    }
}
