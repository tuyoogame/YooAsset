using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 表格行数据抽象的默认实现，提供便捷的单元格添加方法
    /// </summary>
    public class DefaultTableData : ITableData
    {
        /// <inheritdoc/>
        public bool Visible { set; get; } = true;

        /// <inheritdoc/>
        public IList<ITableCell> Cells { get; } = new List<ITableCell>();


        /// <summary>
        /// 添加自定义单元格
        /// </summary>
        /// <param name="cell">要添加的单元格实例</param>
        public void AddCell(ITableCell cell)
        {
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加按钮单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        public void AddButtonCell(string searchTag)
        {
            var cell = new ButtonCell(searchTag);
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加资源路径单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="assetPath">资源在项目中的路径</param>
        public void AddAssetPathCell(string searchTag, string assetPath)
        {
            var cell = new AssetPathCell(searchTag, assetPath);
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加资源对象单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="assetPath">资源在项目中的路径</param>
        public void AddAssetObjectCell(string searchTag, string assetPath)
        {
            var cell = new AssetObjectCell(searchTag, assetPath);
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加字符串单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="value">字符串值</param>
        public void AddStringValueCell(string searchTag, string value)
        {
            var cell = new StringValueCell(searchTag, value);
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加长整数单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="value">长整数值</param>
        public void AddLongValueCell(string searchTag, long value)
        {
            var cell = new IntegerValueCell(searchTag, value);
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加双精度浮点单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="value">双精度浮点值</param>
        public void AddDoubleValueCell(string searchTag, double value)
        {
            var cell = new SingleValueCell(searchTag, value);
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加布尔单元格
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="value">布尔值</param>
        public void AddBoolValueCell(string searchTag, bool value)
        {
            var cell = new BooleanValueCell(searchTag, value);
            Cells.Add(cell);
        }
    }
}