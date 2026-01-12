using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源路径类型的表格单元格
    /// </summary>
    public class AssetPathCell : StringValueCell
    {
        /// <summary>
        /// 创建资源路径单元格实例
        /// </summary>
        /// <param name="searchTag">用于搜索匹配的标签标识</param>
        /// <param name="cellValue">资源在项目中的路径</param>
        public AssetPathCell(string searchTag, object cellValue) : base(searchTag, cellValue)
        {
        }

        /// <summary>
        /// 在 Project 窗口中定位并高亮资源对象
        /// </summary>
        /// <returns>定位成功返回 true；资源不存在时返回 false。</returns>
        public bool PingAssetObject()
        {
            var assetPath = StringValue;
            var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(assetGUID))
                return false;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
                return false;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return true;
        }
    }
}