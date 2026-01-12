
namespace YooAsset.Editor
{
    /// <summary>
    /// 搜索关键字命令，按字符串包含关系进行匹配
    /// </summary>
    public class SearchKeyword : ISearchCommand
    {
        /// <inheritdoc/>
        public string SearchTag { get; set; }

        /// <summary>
        /// 待匹配的关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 检查是否包含关键字
        /// </summary>
        /// <param name="value">待比较的字符串</param>
        /// <returns>包含关键字时返回 true</returns>
        public bool CompareTo(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            if (string.IsNullOrEmpty(Keyword))
                return false;
            return value.Contains(Keyword);
        }
    }
}