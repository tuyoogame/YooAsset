using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 表格列定义
    /// </summary>
    public class TableColumn
    {
        /// <summary>
        /// 单元列索引值
        /// </summary>
        internal int ColumnIndex;

        /// <summary>
        /// 单元元素集合
        /// </summary>
        internal List<VisualElement> CellElements = new List<VisualElement>(1000);
        
        /// <summary>
        /// UI元素名称
        /// </summary>
        public string ElementName { private set; get; }

        /// <summary>
        /// 标题名称
        /// </summary>
        public string HeaderTitle { private set; get; }

        /// <summary>
        /// 单元列样式
        /// </summary>
        public ColumnStyle ColumnStyle { private set; get; }

        /// <summary>
        /// 制作单元格元素
        /// </summary>
        public Func<VisualElement> MakeCell { get; set; }

        /// <summary>
        /// 绑定数据到单元格
        /// </summary>
        public Action<VisualElement, ITableData, ITableCell> BindCell { get; set; }

        /// <summary>
        /// 创建表格列实例
        /// </summary>
        /// <param name="elementName">列对应的 UI 元素名称</param>
        /// <param name="headerTitle">列标题栏显示文本</param>
        /// <param name="columnStyle">列的布局与行为样式</param>
        public TableColumn(string elementName, string headerTitle, ColumnStyle columnStyle)
        {
            this.ElementName = elementName;
            this.HeaderTitle = headerTitle;
            this.ColumnStyle = columnStyle;
        }
    }
}