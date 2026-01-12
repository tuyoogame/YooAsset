using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 表格列的布局与行为样式配置
    /// </summary>
    public class ColumnStyle
    {
        /// <summary>
        /// 列宽度允许的最大值
        /// </summary>
        public const float MaxValue = 8388608f;

        /// <summary>
        /// 单元列宽度
        /// </summary>
        public Length Width { get; set; }

        /// <summary>
        /// 单元列最小宽度
        /// </summary>
        public Length MinWidth { get; set; }

        /// <summary>
        /// 单元列最大宽度
        /// </summary>
        public Length MaxWidth { get; set; }

        /// <summary>
        /// 是否可伸缩
        /// </summary>
        public bool Stretchable { get; set; } = false;

        /// <summary>
        /// 是否可搜索
        /// </summary>
        public bool Searchable { get; set; } = false;

        /// <summary>
        /// 是否可排序
        /// </summary>
        public bool Sortable { get; set; } = false;

        /// <summary>
        /// 是否在标题栏显示数据总数
        /// </summary>
        public bool Counter { get; set; } = false;

        /// <summary>
        /// 展示单位
        /// </summary>
        public string Units { get; set; } = string.Empty;

        /// <summary>
        /// 创建固定宽度的列样式实例
        /// </summary>
        /// <param name="width">列宽度，同时作为最小和最大宽度</param>
        public ColumnStyle(Length width)
        {
            if (width.value > MaxValue)
                width = MaxValue;

            Width = width;
            MinWidth = width;
            MaxWidth = width;
        }
        /// <summary>
        /// 创建可变宽度的列样式实例
        /// </summary>
        /// <param name="width">列的初始宽度</param>
        /// <param name="minWidth">列的最小宽度</param>
        /// <param name="maxWidth">列的最大宽度</param>
        public ColumnStyle(Length width, Length minWidth, Length maxWidth)
        {
            if (maxWidth.value > MaxValue)
                maxWidth = MaxValue;

            Width = width;
            MinWidth = minWidth;
            MaxWidth = maxWidth;
        }
    }
}