
namespace YooAsset.Editor
{
#if UNITY_2019
    /// <summary>
    /// ListView 的兼容扩展
    /// </summary>
    public static partial class ListViewCompatExtension
    {
        /// <summary>
        /// 清除当前选中项
        /// </summary>
        /// <param name="o">目标 ListView</param>
        public static void ClearSelection(this UnityEngine.UIElements.ListView o)
        {
            o.selectedIndex = -1;
        }
    }
#endif

#if UNITY_2019 || UNITY_2020
    /// <summary>
    /// ListView 的兼容扩展
    /// </summary>
    public static partial class ListViewCompatExtension
    {
        /// <summary>
        /// 重建列表视图
        /// </summary>
        /// <param name="o">目标 ListView</param>
        public static void Rebuild(this UnityEngine.UIElements.ListView o)
        {
            o.Refresh();
        }
    }
#endif
}