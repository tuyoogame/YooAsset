using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 自定义下载器的请求委托
    /// </summary>
    public delegate UnityWebRequest UnityWebRequestDelegate(string url);

    internal class DownloadSystemHelper
    {
        public static UnityWebRequestDelegate UnityWebRequestCreater = null;
        public static UnityWebRequest NewUnityWebRequestGet(string requestURL)
        {
            UnityWebRequest webRequest;
            if (UnityWebRequestCreater != null)
                webRequest = UnityWebRequestCreater.Invoke(requestURL);
            else
                webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbGET);
            return webRequest;
        }

        /// <summary>
        /// 获取WWW加载本地资源的路径
        /// </summary>
        public static string ConvertToWWWPath(string path)
        {
            string urlPath;
#if UNITY_EDITOR
            urlPath = StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
            urlPath = path;
#elif UNITY_IPHONE
            urlPath = StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
            if (path.StartsWith("jar:file://"))
                urlPath = path;
            else
                urlPath = StringUtility.Format("jar:file://{0}", path);
#elif UNITY_STANDALONE_OSX
            urlPath = new System.Uri(path).ToString();
#elif UNITY_STANDALONE
            urlPath = StringUtility.Format("file:///{0}", path);
#elif UNITY_OPENHARMONY
            urlPath = StringUtility.Format("file://{0}", path);
#else
            throw new System.NotImplementedException();
#endif
            return urlPath.Replace("+", "%2B").Replace("#", "%23").Replace("?", "%3F");
        }
    }
}