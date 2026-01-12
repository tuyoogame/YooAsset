using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 布尔类型的表格单元格
    /// </summary>
    public class BooleanValueCell : ITableCell, IComparable
    {
        /// <inheritdoc/>
        public object CellValue { set; get; }

        /// <summary>
        /// 搜索标签
        /// </summary>
        public string SearchTag { private set; get; }

        /// <summary>
        /// 布尔形式的单元格值
        /// </summary>
        public bool BooleanValue
        {
            get
            {
                return (bool)CellValue;
            }
        }

        /// <summary>
        /// 创建布尔单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="cellValue">单元格存储的布尔值</param>
        public BooleanValueCell(string searchTag, object cellValue)
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
            if (other is BooleanValueCell cell)
            {
                return this.BooleanValue.CompareTo(cell.BooleanValue);
            }
            else
            {
                return 0;
            }
        }
    }
}