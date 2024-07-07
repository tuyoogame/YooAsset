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
#if UNITY_EDITOR
            return StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
            return path;
#elif UNITY_IPHONE
            return StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
            if (path.StartsWith("jar:file://"))
                return path;
            else
                return StringUtility.Format("jar:file://{0}", path);
#elif UNITY_STANDALONE_OSX
            return new System.Uri(path).ToString();
#elif UNITY_STANDALONE
            return StringUtility.Format("file:///{0}", path);
#elif UNITY_OPENHARMONY
            return StringUtility.Format("file://{0}", path);
#else
            throw new System.NotImplementedException(); 
#endif
        }
    }
}