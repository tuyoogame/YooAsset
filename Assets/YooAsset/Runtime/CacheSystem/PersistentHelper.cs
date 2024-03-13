
namespace YooAsset
{
    internal static class PersistentHelper
    {
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
            return path;
#else
            return path;
#endif
        }
    }
}