
namespace YooAsset.Editor
{
    /// <summary>
    /// 表格单元格的数据抽象
    /// </summary>
    public interface ITableCell
    {
        /// <summary>
        /// 单元格数值
        /// </summary>
        object CellValue { set; get; }

        /// <summary>
        /// 获取用于界面渲染的显示对象
        /// </summary>
        /// <returns>可用于 UI 绑定的对象实例</returns>
        object GetDisplayObject();
    }
}