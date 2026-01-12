
namespace YooAsset
{
    /// <summary>
    /// 文本工具类
    /// </summary>
    internal static class TextUtility
    {
        /// <summary>
        /// 严格校验文本文件内容是否合法
        /// </summary>
        /// <param name="content">待校验的文本内容</param>
        /// <param name="error">校验失败时的错误描述</param>
        /// <returns>校验通过返回true，否则返回false</returns>
        public static bool ValidateContent(string content, out string error)
        {
            if (string.IsNullOrEmpty(content))
            {
                error = "Text content is null or empty.";
                return false;
            }

            // 检验BOM
            if (content[0] == '\uFEFF')
            {
                error = "Text content contains a UTF-8 BOM.";
                return false;
            }

            // 检验换行符
            if (content.IndexOf('\r') >= 0 || content.IndexOf('\n') >= 0)
            {
                error = "Text content contains line break characters.";
                return false;
            }

            // 检验首尾空白
            if (char.IsWhiteSpace(content[0]) || char.IsWhiteSpace(content[content.Length - 1]))
            {
                error = "Text content contains leading or trailing whitespace.";
                return false;
            }

            error = null;
            return true;
        }
    }
}