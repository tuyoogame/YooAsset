using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 下载器基类
    /// </summary>
    /// <remarks>
    /// <para>封装 UnityWebRequest 的通用下载逻辑，包括状态管理、进度追踪等。</para>
    /// <para>子类只需实现 CreateWebRequest 方法来创建特定类型的下载请求</para>
    /// </remarks>
    internal abstract class UnityWebRequestBase : IDownloadRequest
    {
        /// <summary>
        /// 自定义 UnityWebRequest 创建器
        /// </summary>
        private readonly UnityWebRequestCreator _webRequestCreator;

        /// <summary>
        /// 公共请求参数
        /// </summary>
        private readonly DownloadRequestArgs _requestArgs;

        /// <summary>
        /// UnityWebRequest 实例（基类私有持有）
        /// </summary>
        private UnityWebRequest _webRequest;

        /// <summary>
        /// 是否启用看门狗
        /// </summary>
        private bool IsWatchdogEnabled
        {
            get { return _requestArgs.WatchdogTimeout > 0; }
        }

        /// <summary>
        /// 最近一次记录的下载字节数
        /// </summary>
        private long _lastDownloadBytes;

        /// <summary>
        /// 最近一次接收数据的时间
        /// </summary>
        private double _lastDataReceivedTime;

        #region 接口实现
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        // TODO: 该属性具有轮询驱动语义，每次访问会推进内部状态机。后续优化为无副作用的纯查询属性。
        public bool IsDone
        {
            get
            {
                PollRequest();
                return Status == EDownloadRequestStatus.Succeeded
                    || Status == EDownloadRequestStatus.Failed;
            }
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public EDownloadRequestStatus Status { get; protected set; }

        /// <summary>
        /// 当前下载进度（0f - 1f）
        /// </summary>
        public float DownloadProgress { get; private set; }

        /// <summary>
        /// 当前请求已接收的字节数
        /// </summary>
        public long DownloadedBytes { get; private set; }

        /// <summary>
        /// HTTP 返回码
        /// </summary>
        public long HttpCode { get; private set; }

        /// <summary>
        /// HTTP 错误信息
        /// </summary>
        public string HttpError { get; private set; }

        /// <summary>
        /// 请求失败时的错误信息
        /// </summary>
        /// <remarks>
        /// 仅在请求失败时有意义；成功或尚未完成时通常为空。
        /// </remarks>
        public string Error { get; protected set; }
        #endregion

        /// <summary>
        /// 构造下载器基类
        /// </summary>
        /// <param name="requestArgs">公共请求参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        protected UnityWebRequestBase(DownloadRequestArgs requestArgs, UnityWebRequestCreator webRequestCreator)
        {
            _requestArgs = requestArgs;
            _webRequestCreator = webRequestCreator;
            Url = requestArgs.Url;
            Status = EDownloadRequestStatus.None;
        }

        /// <summary>
        /// 发起请求
        /// </summary>
        /// <remarks>
        /// <para>仅在 Status 为 None 时生效，重复调用无效。</para>
        /// <para>调用后 Status 变为 Running</para>
        /// </remarks>
        public void SendRequest()
        {
            if (Status == EDownloadRequestStatus.None)
            {
                Status = EDownloadRequestStatus.Running;

                _lastDataReceivedTime = TimeUtility.RealtimeSinceStartup;
                _lastDownloadBytes = 0;

                try
                {
                    _webRequest = CreateWebRequest();
                    if (_webRequest == null)
                    {
                        Status = EDownloadRequestStatus.Failed;
                        Error = $"[{GetType().Name}] CreateWebRequest() returned null.";
                    }
                    else
                    {
                        ApplyRequestConfig();
                        _webRequest.SendWebRequest();
                    }
                }
                catch (Exception ex)
                {
                    Status = EDownloadRequestStatus.Failed;
                    Error = $"[{GetType().Name}] Failed to create web request: {ex.Message}.";
                }
            }
        }

        /// <summary>
        /// 中止请求
        /// </summary>
        /// <remarks>
        /// 可在任意状态调用，仅当 Status 为 None 或 Running 时生效。
        /// </remarks>
        public void AbortRequest()
        {
            if (Status == EDownloadRequestStatus.None)
            {
                Status = EDownloadRequestStatus.Failed;
                Error = $"[{GetType().Name}] Request canceled. URL: '{Url}'.";
            }
            else if (Status == EDownloadRequestStatus.Running)
            {
                // 注意：Abort并不会立即终止网络请求
                // If the UnityWebRequest has not already completed, the UnityWebRequest will halt uploading or downloading data as soon as possible.
                Status = EDownloadRequestStatus.Aborting;
                if (_webRequest != null)
                    _webRequest.Abort();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            CleanupWebRequest();
        }


        /// <summary>
        /// 创建 UnityWebRequest（子类实现）
        /// </summary>
        /// <returns>配置好 handler 的 UnityWebRequest 实例</returns>
        protected abstract UnityWebRequest CreateWebRequest();

        /// <summary>
        /// 请求成功时的回调（子类可重写）
        /// </summary>
        /// <param name="webRequest">已完成的 UnityWebRequest 实例，可用于读取响应数据。</param>
        protected virtual void OnRequestSucceeded(UnityWebRequest webRequest)
        {
        }

        /// <summary>
        /// 请求失败时的回调（子类可重写）
        /// </summary>
        /// <param name="webRequest">已完成的 UnityWebRequest 实例，可用于读取错误详情。</param>
        protected virtual void OnRequestFailed(UnityWebRequest webRequest)
        {
        }


        /// <summary>
        /// 创建 UnityWebRequest GET 请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <returns>UnityWebRequest 实例</returns>
        protected UnityWebRequest CreateGetWebRequest(string requestUrl)
        {
            if (_webRequestCreator != null)
                return _webRequestCreator.Invoke(requestUrl, UnityWebRequest.kHttpVerbGET);

            return new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbGET);
        }

        /// <summary>
        /// 创建 UnityWebRequest HEAD 请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <returns>UnityWebRequest 实例</returns>
        protected UnityWebRequest CreateHeadWebRequest(string requestUrl)
        {
            if (_webRequestCreator != null)
                return _webRequestCreator.Invoke(requestUrl, UnityWebRequest.kHttpVerbHEAD);

            return new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbHEAD);
        }


        /// <summary>
        /// 更新网络请求
        /// </summary>
        private void PollRequest()
        {
            if (Status != EDownloadRequestStatus.Running && Status != EDownloadRequestStatus.Aborting)
                return;

            DownloadProgress = _webRequest.downloadProgress;
            DownloadedBytes = (long)_webRequest.downloadedBytes;

            UpdateWatchdog();
            if (_webRequest.isDone == false)
                return;

            HttpCode = _webRequest.responseCode;
            HttpError = _webRequest.error;

#if UNITY_2020_3_OR_NEWER
            bool isSuccess = _webRequest.result == UnityWebRequest.Result.Success;
#else
            bool isSuccess = !_webRequest.isNetworkError && !_webRequest.isHttpError;
#endif

            // 注意：Aborting 状态下也可能进入此分支。
            // 当看门狗触发 Abort 时，如果请求恰好已在引擎内部完成，isDone 和 isSuccess 均为 true。
            // 此时数据已完整下载，标记为 Succeeded 在语义上是合理的（Unity 文档：已完成的请求 Abort 无效）。
            if (isSuccess)
            {
                Status = EDownloadRequestStatus.Succeeded;
                OnRequestSucceeded(_webRequest);
            }
            else
            {
                Status = EDownloadRequestStatus.Failed;
                Error = $"[{GetType().Name}] Request failed. URL: '{Url}', HttpCode={HttpCode}, HttpError={HttpError}.";
                OnRequestFailed(_webRequest);
            }

            // 完成后释放
            CleanupWebRequest();
        }

        /// <summary>
        /// 更新看门狗机制
        /// </summary>
        private void UpdateWatchdog()
        {
            if (!IsWatchdogEnabled)
                return;
            if (Status != EDownloadRequestStatus.Running)
                return;

            double realtimeSinceStartup = TimeUtility.RealtimeSinceStartup;
            if (DownloadedBytes != _lastDownloadBytes)
            {
                _lastDownloadBytes = DownloadedBytes;
                _lastDataReceivedTime = realtimeSinceStartup;
            }
            else
            {
                double deltaTime = realtimeSinceStartup - _lastDataReceivedTime;
                if (deltaTime > _requestArgs.WatchdogTimeout)
                {
                    AbortRequest();
                    YooLogger.LogWarning($"Watchdog timeout after {deltaTime:F1}s for '{Url}'.");
                }
            }
        }

        /// <summary>
        /// 清理 WebRequest 资源
        /// </summary>
        private void CleanupWebRequest()
        {
            if (_webRequest != null)
            {
                //注意：引擎底层会自动调用Abort方法
                _webRequest.Dispose();
                _webRequest = null;
            }
        }

        /// <summary>
        /// 应用公共参数
        /// </summary>
        private void ApplyRequestConfig()
        {
            if (_requestArgs.Timeout > 0)
                _webRequest.timeout = _requestArgs.Timeout;

            if (_requestArgs.Headers != null)
            {
                foreach (var header in _requestArgs.Headers)
                {
                    _webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }
        }
    }
}