
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

            string url;
            if (IsLocalFileUrl(filePath))
                url = filePath;
            else
                url = CreateLocalFileUrl(filePath);

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

        private static string CreateLocalFileUrl(string filePath)
        {
#if UNITY_WEBGL
            return filePath;
#elif UNITY_OPENHARMONY
            // 注意：由于鸿蒙系统的特殊性，需要判断双形态
            if (UnityEngine.Application.streamingAssetsPath.StartsWith("jar:file://"))
                return $"jar:file://{filePath}";
            else
                return new System.Uri(filePath).ToString();
#else
            return new System.Uri(filePath).ToString();
#endif
        }
        private static string EscapeSpecialCharacters(string url)
        {
            // 处理特殊字符：用户设备路径可能包含特殊字符导致 URL 无法正确识别
            return url.Replace("+", "%2B").Replace("#", "%23").Replace("?", "%3F");
        }
    }
}
