
namespace YooAsset.Editor
{
    /// <summary>
    /// 资源分组激活规则的输入数据
    /// </summary>
    public readonly struct GroupActiveRuleData
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 创建 GroupActiveRuleData 实例
        /// </summary>
        /// <param name="groupName">分组名称</param>
        public GroupActiveRuleData(string groupName)
        {
            GroupName = groupName;
        }
    }

    /// <summary>
    /// 资源分组激活规则接口
    /// </summary>
    public interface IGroupActiveRule
    {
        /// <summary>
        /// 检查是否激活分组
        /// </summary>
        /// <param name="data">分组数据</param>
        /// <returns>如果分组激活返回 true</returns>
        bool IsActiveGroup(GroupActiveRuleData data);
    }
}
