
namespace YooAsset
{
    /// <summary>
    /// 下载 URL 工具类
    /// </summary>
    internal static class DownloadUrlHelper
    {
        /// <summary>
        /// 将本地文件路径转换为 UnityWebRequest 可用的 URL
        /// </summary>
        /// <param name="filePath">本地文件路径</param>
        /// <returns>可用于 UnityWebRequest 的文件协议 URL</returns>
        /// <remarks>
        /// 不支持 content:// 等文档 URI
        /// </remarks>
        public static string ToLocalFileUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new YooInternalException("File path is null or empty.");

            if (IsLocalFileUrl(filePath))
                return EscapeSpecialCharacters(filePath);

            string url;

#if UNITY_WEBGL
            url = filePath;
#elif UNITY_ANDROID
            url = new System.Uri(filePath).ToString();
#elif UNITY_OPENHARMONY
            // 注意：由于鸿蒙系统的特殊性，需要判断双形态
            if (UnityEngine.Application.streamingAssetsPath.StartsWith("jar:file://"))
                url = StringUtility.Format("jar:file://{0}", filePath);
            else
                url = new System.Uri(filePath).ToString();
#else
            url = new System.Uri(filePath).ToString();
#endif

            return EscapeSpecialCharacters(url);
        }

        /// <summary>
        /// 判断是否为本地文件 URL
        /// </summary>
        /// <param name="url">要判断的 URL</param>
        /// <returns>如果是本地文件 URL 返回 true，否则返回 false。</returns>
        public static bool IsLocalFileUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            if (url.StartsWith("file://"))
                return true;

            if (url.StartsWith("jar:file://"))
                return true;

            return false;
        }

        private static string EscapeSpecialCharacters(string url)
        {
            // 处理特殊字符：用户设备路径可能包含特殊字符导致 URL 无法正确识别
            return url.Replace("+", "%2B").Replace("#", "%23").Replace("?", "%3F");
        }
    }
}
