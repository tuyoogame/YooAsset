using UnityEditor;
using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源对象类型的表格单元格
    /// </summary>
    public class AssetObjectCell : ITableCell, IComparable
    {
        private UnityEngine.Object _cachedDisplayObject;

        /// <inheritdoc/>
        public object CellValue { set; get; }

        /// <summary>
        /// 搜索标签
        /// </summary>
        public string SearchTag { private set; get; }

        /// <summary>
        /// 字符串形式的资源路径
        /// </summary>
        public string StringValue
        {
            get
            {
                return (string)CellValue;
            }
        }

        /// <summary>
        /// 创建资源对象单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="assetPath">资源在项目中的路径</param>
        public AssetObjectCell(string searchTag, string assetPath)
        {
            SearchTag = searchTag;
            CellValue = assetPath;
        }

        /// <inheritdoc/>
        public object GetDisplayObject()
        {
            if (_cachedDisplayObject == null)
                _cachedDisplayObject = AssetDatabase.LoadMainAssetAtPath(StringValue);
            return _cachedDisplayObject;
        }

        /// <inheritdoc/>
        public int CompareTo(object other)
        {
            if (other is AssetObjectCell cell)
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