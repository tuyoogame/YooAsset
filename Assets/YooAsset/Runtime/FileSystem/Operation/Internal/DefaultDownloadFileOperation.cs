using UnityEngine.Networking;

namespace YooAsset
{
    internal abstract class DefaultDownloadFileOperation : FSDownloadFileOperation
    {
        protected enum ESteps
        {
            None,
            CreateRequest,
            CheckRequest,
            VerifyTempFile,
            CheckVerifyTempFile,
            TryAgain,
            Done,
        }

        // 初始参数
        protected readonly string _mainURL;
        protected readonly string _fallbackURL;
        protected readonly int _failedTryAgain;
        protected readonly int _timeout;

        // 请求相关
        protected UnityWebRequest _webRequest;
        protected string _requestURL;
        protected int _requestCount = 0;

        // 超时相关
        protected bool _isAbort = false;
        protected long _latestDownloadBytes;
        protected float _latestDownloadRealtime;
        protected float _tryAgainTimer;

        // 失败相关
        protected int FailedTryAgain;


        internal DefaultDownloadFileOperation(PackageBundle bundle,
            string mainURL, string fallbackURL, int failedTryAgain, int timeout) : base(bundle)
        {
            _mainURL = mainURL;
            _fallbackURL = fallbackURL;
            _failedTryAgain = failedTryAgain;
            _timeout = timeout;

            FailedTryAgain = failedTryAgain;
        }

        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            _requestCount++;
            if (_requestCount % 2 == 0)
                return _fallbackURL;
            else
                return _mainURL;
        }

        /// <summary>
        /// 检测请求超时
        /// </summary>
        protected void CheckRequestTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (_isAbort == false)
            {
                if (_latestDownloadBytes != DownloadedBytes)
                {
                    _latestDownloadBytes = DownloadedBytes;
                    _latestDownloadRealtime = UnityEngine.Time.realtimeSinceStartup;
                }

                float offset = UnityEngine.Time.realtimeSinceStartup - _latestDownloadRealtime;
                if (offset > _timeout)
                {
                    YooLogger.Warning($"Web file request timeout : {_requestURL}");
                    if (_webRequest != null)
                        _webRequest.Abort();
                    _isAbort = true;
                }
            }
        }

        /// <summary>
        /// 检测请求结果
        /// </summary>
        protected bool CheckRequestResult()
        {
            HttpCode = _webRequest.responseCode;

#if UNITY_2020_3_OR_NEWER
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                Error = _webRequest.error;
                return false;
            }
            else
            {
                return true;
            }
#else
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
            {
                Error = _webRequest.error;
                return false;
            }
            else
            {
                return true;
            }
#endif
        }
    }
}