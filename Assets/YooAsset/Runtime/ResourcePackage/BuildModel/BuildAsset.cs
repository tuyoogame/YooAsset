namespace YooAsset
{
    /// <summary>
    /// 构建期专用的资源描述
    /// </summary>
    internal class BuildAsset
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
        /// 所属资源包ID
        /// </summary>
        public int BundleID;

        /// <summary>
        /// 依赖的资源包ID集合
        /// 说明：框架层收集查询结果
        /// </summary>
        public int[] DependentBundleIDs;

        /// <summary>
        /// 资源的分类标签
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 临时数据对象
        /// </summary>
        public object EditorUserData;
    }
}
