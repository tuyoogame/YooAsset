using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 字符串类型的表格单元格
    /// </summary>
    public class StringValueCell : ITableCell, IComparable
    {
        /// <inheritdoc/>
        public object CellValue { set; get; }

        /// <summary>
        /// 搜索标签
        /// </summary>
        public string SearchTag { private set; get; }

        /// <summary>
        /// 字符串形式的单元格值
        /// </summary>
        public string StringValue
        {
            get
            {
                return (string)CellValue;
            }
        }
        
        /// <summary>
        /// 创建字符串单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="cellValue">单元格存储的字符串值</param>
        public StringValueCell(string searchTag, object cellValue)
        {
            SearchTag = searchTag;
            CellValue = cellValue;
        }

        /// <inheritdoc/>
        public object GetDisplayObject()
        {
            return CellValue;
        }

        /// <inheritdoc/>
        public int CompareTo(object other)
        {
            if (other is StringValueCell cell)
            {
                return this.StringValue.CompareTo(cell.StringValue);
            }
            else
            {
                return 0;
            }
        }
    }
}