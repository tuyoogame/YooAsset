using System;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 路径工具类
    /// </summary>
    internal static class PathUtility
    {
        /// <summary>
        /// 将路径中的反斜杠替换为正斜杠
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// 移除路径里的后缀名
        /// </summary>
        public static string RemoveExtension(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // 定位最后一个路径分隔符，确保只移除文件名部分的扩展名。
            // 避免目录名含点时（如 "com.unity.render/shader"）被错误截断。
            int sepIndex = Math.Max(value.LastIndexOf('/'), value.LastIndexOf('\\'));
            int dotIndex = value.LastIndexOf('.');
            if (dotIndex == -1 || dotIndex < sepIndex)
                return value;
            return value.Remove(dotIndex);
        }

        /// <summary>
        /// 检查文件名是否包含非法字符
        /// </summary>
        /// <param name="fileName">待检查的文件名</param>
        /// <returns>包含非法字符返回 true，否则返回 false</returns>
        public static bool ContainsInvalidFileNameChars(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
        }

        /// <summary>
        /// 检查URL地址在协议之后的部分是否包含双斜杠
        /// </summary>
        public static bool ContainsDoubleSlashes(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            int protocolIndex = url.IndexOf("://");
            string partToCheck = protocolIndex == -1 ? url : url.Substring(protocolIndex + 3);
            return partToCheck.Contains("//") || partToCheck.Contains(@"\\");
        }

        /// <summary>
        /// 合并路径
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            // 注意：某些静态服务器的规则可能不接受双斜杠路径
            if (path1 == null)
                throw new ArgumentNullException(nameof(path1));
            if (path2 == null)
                throw new ArgumentNullException(nameof(path2));
            return StringUtility.Format("{0}/{1}", path1.TrimEnd('/'), path2.Trim('/'));
        }

        /// <summary>
        /// 合并路径
        /// </summary>
        public static string Combine(string path1, string path2, string path3)
        {
            // 注意：某些静态服务器的规则可能不接受双斜杠路径
            if (path1 == null)
                throw new ArgumentNullException(nameof(path1));
            if (path2 == null)
                throw new ArgumentNullException(nameof(path2));
            if (path3 == null)
                throw new ArgumentNullException(nameof(path3));
            return StringUtility.Format("{0}/{1}/{2}", path1.TrimEnd('/'), path2.Trim('/'), path3.Trim('/'));
        }

        /// <summary>
        /// 合并路径
        /// </summary>
        public static string Combine(string path1, string path2, string path3, string path4)
        {
            // 注意：某些静态服务器的规则可能不接受双斜杠路径
            if (path1 == null)
                throw new ArgumentNullException(nameof(path1));
            if (path2 == null)
                throw new ArgumentNullException(nameof(path2));
            if (path3 == null)
                throw new ArgumentNullException(nameof(path3));
            if (path4 == null)
                throw new ArgumentNullException(nameof(path4));
            return StringUtility.Format("{0}/{1}/{2}/{3}", path1.TrimEnd('/'), path2.Trim('/'), path3.Trim('/'), path4.Trim('/'));
        }
    }
}