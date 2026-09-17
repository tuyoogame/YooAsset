using System;
using System.Collections.Generic;
using System.Globalization;

namespace YooAsset.Editor
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class EditorStringUtility
    {
        /// <summary>
        /// 检测格式控制字符
        /// </summary>
        /// <param name="value">待检测的字符串</param>
        /// <returns>首个命中字符的转义文本</returns>
        public static string GetFormatCharacterEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            for (int index = 0; index < value.Length;)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(value, index) == UnicodeCategory.Format)
                {
                    int codePoint = char.ConvertToUtf32(value, index);
                    if (codePoint > 0xFFFF)
                        return @"\U" + codePoint.ToString("X8", CultureInfo.InvariantCulture);
                    else
                        return @"\u" + codePoint.ToString("X4", CultureInfo.InvariantCulture);
                }
                index += char.IsSurrogatePair(value, index) ? 2 : 1;
            }

            return null;
        }

        /// <summary>
        /// 移除字符串的第一个字符
        /// </summary>
        /// <param name="value">原始字符串</param>
        /// <returns>去除首字符后的字符串</returns>
        public static string RemoveFirstChar(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Substring(1);
        }

        /// <summary>
        /// 移除字符串的最后一个字符
        /// </summary>
        /// <param name="value">原始字符串</param>
        /// <returns>去除尾字符后的字符串</returns>
        public static string RemoveLastChar(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Substring(0, value.Length - 1);
        }

        /// <summary>
        /// 按分隔符拆分字符串并去除空白项
        /// </summary>
        /// <param name="value">待拆分的字符串</param>
        /// <param name="separator">分隔字符</param>
        /// <returns>拆分后的字符串列表</returns>
        public static List<string> SplitToList(string value, char separator)
        {
            List<string> result = new List<string>();
            if (!String.IsNullOrEmpty(value))
            {
                string[] splits = value.Split(separator);
                foreach (string split in splits)
                {
                    string item = split.Trim();
                    if (!String.IsNullOrEmpty(item))
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 将字符串解析为指定的枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="name">枚举成员名称</param>
        /// <returns>解析后的枚举值</returns>
        public static T ParseEnum<T>(string name)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), name);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Name '{name}' is not defined in enum {typeof(T)}.");
            }
        }

        /// <summary>
        /// 截取字符串中匹配关键字之后的内容
        /// </summary>
        /// <param name="content">原始内容</param>
        /// <param name="key">关键字</param>
        /// <param name="includeKey">结果中是否包含关键字</param>
        /// <param name="firstMatch">是否使用第一次匹配的位置，为 false 时使用最后一次匹配的位置</param>
        /// <returns>截取后的子字符串，未匹配时为原始内容</returns>
        public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
        {
            if (string.IsNullOrEmpty(key))
                return content;

            int startIndex = -1;
            if (firstMatch)
                startIndex = content.IndexOf(key);
            else
                startIndex = content.LastIndexOf(key);

            if (startIndex == -1)
                return content;

            if (includeKey)
                return content.Substring(startIndex);
            else
                return content.Substring(startIndex + key.Length);
        }
    }
}
