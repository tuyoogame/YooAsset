using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 表格行数据的抽象，包含可见性控制和单元格集合
    /// </summary>
    public interface ITableData
    {
        /// <summary>
        /// 是否可见
        /// </summary>
        bool Visible { set; get; }

        /// <summary>
        /// 单元格集合
        /// </summary>
        IList<ITableCell> Cells { get; }
    }
}