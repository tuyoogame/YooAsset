
namespace YooAsset.Editor
{
    public struct FilterRuleData
    {
        public string AssetPath;
        public string CollectPath;
        public string GroupName;
        public string UserData;

        public FilterRuleData(string assetPath, string collectPath, string groupName, string userData)
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
    public interface IFilterRule
    {
        /// <summary>
        /// 搜寻的资源类型
        /// 说明：使用引擎方法搜索获取所有资源列表
        /// </summary>
        string FindAssetType { get; }

        /// <summary>
        /// 验证搜寻的资源是否为收集资源
        /// </summary>
        /// <returns>如果收集该资源返回TRUE</returns>
        bool IsCollectAsset(FilterRuleData data);
    }
}