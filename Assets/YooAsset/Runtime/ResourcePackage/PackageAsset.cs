using System;
using System.Linq;

namespace YooAsset
{
    /// <summary>
    /// 清单中的资源描述
    /// </summary>
    [Serializable]
    internal class PackageAsset
    {
        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源GUID
        /// </summary>
        public string AssetGuid;

        /// <summary>
        /// 资源的分类标签
        /// </summary>
        public string[] AssetTags;

        /// <summary>
        /// 所属资源包ID
        /// </summary>
        public int BundleID;

        /// <summary>
        /// 依赖的资源包ID集合
        /// 说明：框架层收集查询结果
        /// </summary>
        public int[] DependentBundleIDs;

        /// <summary>
        /// 临时数据对象（仅编辑器有效）
        /// </summary>
        [NonSerialized]
        public object EditorUserData;

        /// <summary>
        /// 是否包含指定的标签
        /// </summary>
        /// <param name="tags">要检查的标签数组</param>
        /// <returns>如果包含任意一个标签返回true，否则返回false。</returns>
        public bool HasAnyTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;
            if (AssetTags == null || AssetTags.Length == 0)
                return false;

            foreach (var tag in tags)
            {
                if (AssetTags.Contains(tag))
                    return true;
            }
            return false;
        }
    }
}