using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal abstract class DefaultDownloadFileOperation : FSDownloadFileOperation
    {
        protected enum ESteps
        {
            None,
            CheckExists,
            CreateRequest,
            CheckRequest,
            VerifyTempFile,
            CheckVerifyTempFile,
            TryAgain,
            Done,
        }

        // 下载参数
        protected readonly DownloadParam Param;

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


        internal DefaultDownloadFileOperation(PackageBundle bundle, DownloadParam param) : base(bundle)
        {
            Param = param;
            FailedTryAgain = param.FailedTryAgain;
        }

        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            _requestCount++;
            if (_requestCount % 2 == 0)
                return Param.FallbackURL;
            else
                return Param.MainURL;
        }

        /// <summary>
        /// 重置请求字段
        /// </summary>
        protected void ResetRequestFiled()
        {
            // 重置变量
            _isAbort = false;
            _latestDownloadBytes = 0;
            _latestDownloadRealtime = Time.realtimeSinceStartup;
            DownloadProgress = 0f;
            DownloadedBytes = 0;

            // 重置计时器
            if (_tryAgainTimer > 0f)
                YooLogger.Warning($"Try again download : {_requestURL}");
            _tryAgainTimer = 0f;
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
                if (offset > Param.Timeout)
                {
                    YooLogger.Warning($"Download request timeout : {_requestURL}");
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

        /// <summary>
        /// 是否请求的本地文件
        /// </summary>
        protected bool IsRequestLocalFile()
        {
            //TODO : UNITY_STANDALONE_OSX平台目前无法确定
            if (Param.MainURL.StartsWith("file:"))
                return true;
            if (Param.MainURL.StartsWith("jar:file:"))
                return true;

            return false;
        }
    }
}