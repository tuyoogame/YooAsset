using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 按钮类型的表格单元格
    /// </summary>
    public class ButtonCell : ITableCell, IComparable
    {
        /// <inheritdoc/>
        public object CellValue { set; get; }

        /// <summary>
        /// 搜索标签
        /// </summary>
        public string SearchTag { private set; get; }

        /// <summary>
        /// 创建按钮单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        public ButtonCell(string searchTag)
        {
            SearchTag = searchTag;
        }

        /// <inheritdoc/>
        public object GetDisplayObject()
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public int CompareTo(object other)
        {
            return 0;
        }
    }
}