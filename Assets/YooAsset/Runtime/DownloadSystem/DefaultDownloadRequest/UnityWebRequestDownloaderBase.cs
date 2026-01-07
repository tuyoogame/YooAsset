using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 下载器基类
    /// </summary>
    /// <remarks>
    /// 封装 UnityWebRequest 的通用下载逻辑，包括状态管理、进度追踪等。
    /// 子类只需实现 CreateWebRequest 方法来创建特定类型的下载请求。
    /// </remarks>
    internal abstract class UnityWebRequestDownloaderBase : IDownloadRequest
    {
        private readonly UnityWebRequestCreator _webRequestCreator;
        protected UnityWebRequest _webRequest;

        // 看门狗相关
        private int _watchdogTime = 0;
        private bool _watchdogAborted = false;
        private long _lastDownloadBytes = -1;
        private double _lastGetDataTime;

        #region 接口实现
        /// <summary>
        /// 请求地址
        /// </summary>
        public string URL { get; }

        /// <summary>
        /// 是否完成
        /// </summary>
        /// <remarks>
        /// 每次调用都会主动轮询请求 PollingRequest
        /// </remarks>
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
        /// 错误信息
        /// </summary>
        public string Error { get; protected set; }
        #endregion

        /// <summary>
        /// 构造下载器基类
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        protected UnityWebRequestDownloaderBase(string url, UnityWebRequestCreator webRequestCreator)
        {
            URL = url;
            _webRequestCreator = webRequestCreator;
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

                try
                {
                    CreateWebRequest();

                    if (_webRequest == null)
                    {
                        Status = EDownloadRequestStatus.Failed;
                        Error = $"[{GetType().Name}] Created web request is null.";
                    }
                    else
                    {
                        _webRequest.SendWebRequest();
                    }
                }
                catch (Exception ex)
                {
                    Status = EDownloadRequestStatus.Failed;
                    Error = $"[{GetType().Name}] Failed to create web request : {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 轮询请求
        /// </summary>
        public void PollingRequest()
        {
            if (Status != EDownloadRequestStatus.Running)
                return;

            DownloadProgress = _webRequest.downloadProgress;
            DownloadedBytes = (long)_webRequest.downloadedBytes;

            CheckWatchdog();
            if (_webRequest.isDone == false)
                return;

            HttpCode = _webRequest.responseCode;
#if UNITY_2020_3_OR_NEWER
            bool isSuccess = _webRequest.result == UnityWebRequest.Result.Success;
#else
            bool isSuccess = !_webRequest.isNetworkError && !_webRequest.isHttpError;
#endif

            if (isSuccess)
            {
                Status = EDownloadRequestStatus.Succeed;
                OnRequestSucceed();
            }
            else
            {
                Status = EDownloadRequestStatus.Failed;
                Error = $"[{GetType().Name}] URL: {URL} - 错误: {_webRequest.error}";
                OnRequestFailed();
            }

            // 完成后释放
            DisposeWebRequest();
        }

        /// <summary>
        /// 中止请求
        /// </summary>
        public void AbortRequest()
        {
            if (Status == EDownloadRequestStatus.None || Status == EDownloadRequestStatus.Running)
            {
                Status = EDownloadRequestStatus.Aborted;
                if (_webRequest != null)
                    _webRequest.Abort();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            DisposeWebRequest();
        }


        /// <summary>
        /// 创建 UnityWebRequest（子类实现）
        /// </summary>
        protected abstract void CreateWebRequest();

        /// <summary>
        /// 请求成功时的回调（子类可重写）
        /// </summary>
        protected virtual void OnRequestSucceed()
        {
        }

        /// <summary>
        /// 请求失败时的回调（子类可重写）
        /// </summary>
        protected virtual void OnRequestFailed()
        {
        }


        /// <summary>
        /// 创建 UnityWebRequest GET 请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <returns>UnityWebRequest 实例</returns>
        protected UnityWebRequest CreateUnityWebRequestGet(string requestUrl)
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
        protected UnityWebRequest CreateUnityWebRequestHead(string requestUrl)
        {
            if (_webRequestCreator != null)
                return _webRequestCreator.Invoke(requestUrl, UnityWebRequest.kHttpVerbHEAD);

            return new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbHEAD);
        }

        /// <summary>
        /// 应用通用请求参数
        /// </summary>
        protected void ApplyRequestOptions(int timeout, int watchdogTime, Dictionary<string, string> headers)
        {
            if (_webRequest == null)
                throw new YooInternalException("Web request is null !");

            // 设置看门狗超时时间
            _watchdogTime = watchdogTime;

            // 设置响应的超时时间
            if (timeout > 0)
                _webRequest.timeout = timeout;

            // 设置响应头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    _webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

        /// <summary>
        /// 检测看门狗
        /// </summary>
        private void CheckWatchdog()
        {
            if (_watchdogTime == 0)
                return;
            if (_watchdogAborted)
                return;

#if UNITY_2020_3_OR_NEWER
            double realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartupAsDouble;
#else
            double realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
#endif

            if (DownloadedBytes != _lastDownloadBytes)
            {
                _lastDownloadBytes = DownloadedBytes;
                _lastGetDataTime = realtimeSinceStartup;
            }
            else
            {
                double deltaTime = realtimeSinceStartup - _lastGetDataTime;
                if (deltaTime > _watchdogTime)
                {
                    _watchdogAborted = true;
                    AbortRequest(); //看门狗终止网络请求
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        private void DisposeWebRequest()
        {
            if (_webRequest != null)
            {
                //注意：引擎底层会自动调用Abort方法
                _webRequest.Dispose();
                _webRequest = null;
            }
        }
    }
}
