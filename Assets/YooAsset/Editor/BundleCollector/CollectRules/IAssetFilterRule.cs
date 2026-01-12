
namespace YooAsset.Editor
{
    /// <summary>
    /// 过滤规则的输入数据
    /// </summary>
    public readonly struct AssetFilterRuleData
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 收集路径
        /// </summary>
        public string CollectPath { get; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public string UserData { get; }

        /// <summary>
        /// 创建 AssetFilterRuleData 实例
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="collectPath">收集路径</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="userData">用户自定义数据</param>
        public AssetFilterRuleData(string assetPath, string collectPath, string groupName, string userData)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            UserData = userData;
        }
    }

    /// <summary>
    /// 资源过滤规则接口
    /// </summary>
    public interface IAssetFilterRule
    {
        /// <summary>
        /// 搜寻的资源类型
        /// </summary>
        /// <remarks>使用引擎方法搜索获取所有资源列表</remarks>
        string FindAssetType { get; }

        /// <summary>
        /// 检查是否为收集资源
        /// </summary>
        /// <param name="data">过滤规则数据</param>
        /// <returns>如果收集该资源返回 true</returns>
        bool IsCollectAsset(AssetFilterRuleData data);
    }
}
