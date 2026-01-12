
namespace YooAsset.Editor
{
    /// <summary>
    /// 启用分组的激活规则
    /// </summary>
    [DisplayName("启用分组")]
    public class EnableGroup : IGroupActiveRule
    {
        /// <inheritdoc/>
        public bool IsActiveGroup(GroupActiveRuleData data)
        {
            return true;
        }
    }

    /// <summary>
    /// 禁用分组的激活规则
    /// </summary>
    [DisplayName("禁用分组")]
    public class DisableGroup : IGroupActiveRule
    {
        /// <inheritdoc/>
        public bool IsActiveGroup(GroupActiveRuleData data)
        {
            return false;
        }
    }
}
