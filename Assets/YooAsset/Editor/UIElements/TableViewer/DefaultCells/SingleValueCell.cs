using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 浮点数类型的表格单元格
    /// </summary>
    public class SingleValueCell : ITableCell, IComparable
    {
        /// <inheritdoc/>
        public object CellValue { set; get; }

        /// <summary>
        /// 搜索标签
        /// </summary>
        public string SearchTag { private set; get; }

        /// <summary>
        /// 双精度浮点形式的单元格值
        /// </summary>
        public double SingleValue
        {
            get
            {
                return (double)CellValue;
            }
        }

        /// <summary>
        /// 创建浮点数单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="cellValue">单元格存储的浮点数值</param>
        public SingleValueCell(string searchTag, object cellValue)
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
            if (other is SingleValueCell cell)
            {
                return this.SingleValue.CompareTo(cell.SingleValue);
            }
            else
            {
                return 0;
            }
        }
    }
}