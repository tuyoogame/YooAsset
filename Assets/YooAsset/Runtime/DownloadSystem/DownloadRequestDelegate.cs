using JetBrains.Annotations;
using UnityEngine.Networking;

namespace YooAsset
{
    public delegate UnityWebRequest DownloadRequestDelegate(string url);

    /// <summary>
    /// 自定义下载器的请求
    /// </summary>
    internal static class DownloadRequestUtil
    {
        
        [CanBeNull] private static DownloadRequestDelegate _downloadRequestDelegate;

        public static void SetRequestDelegate(DownloadRequestDelegate requestDelegate)
        {
            _downloadRequestDelegate = requestDelegate;
        }

        public static UnityWebRequest NewRequest(string requestURL)
        {
            return _downloadRequestDelegate != null
                ? _downloadRequestDelegate?.Invoke(requestURL)
                : new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbGET);
        }
    }
}