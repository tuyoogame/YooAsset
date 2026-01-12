using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 整数类型的表格单元格
    /// </summary>
    public class IntegerValueCell : ITableCell, IComparable
    {
        /// <inheritdoc/>
        public object CellValue { set; get; }

        /// <summary>
        /// 搜索标签
        /// </summary>
        public string SearchTag { private set; get; }

        /// <summary>
        /// 整数形式的单元格值
        /// </summary>
        public long IntegerValue
        {
            get
            {
                return (long)CellValue;
            }
        }

        /// <summary>
        /// 创建整数单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="cellValue">单元格存储的整数值</param>
        public IntegerValueCell(string searchTag, object cellValue)
        {
            SearchTag = searchTag;
            CellValue = cellValue;
        }

        /// <inheritdoc/>
        public object GetDisplayObject()
        {
            return CellValue.ToString();
        }

        /// <inheritdoc/>
        public int CompareTo(object other)
        {
            if (other is IntegerValueCell cell)
            {
                return this.IntegerValue.CompareTo(cell.IntegerValue);
            }
            else
            {
                return 0;
            }
        }
    }
}