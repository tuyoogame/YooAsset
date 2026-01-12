
namespace YooAsset.Editor
{
    /// <summary>
    /// 寻址规则的输入数据
    /// </summary>
    public readonly struct AddressRuleData
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
        /// 创建 AddressRuleData 实例
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="collectPath">收集路径</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="userData">用户自定义数据</param>
        public AddressRuleData(string assetPath, string collectPath, string groupName, string userData)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            UserData = userData;
        }
    }

    /// <summary>
    /// 寻址规则接口
    /// </summary>
    public interface IAddressRule
    {
        /// <summary>
        /// 获取资源的寻址地址
        /// </summary>
        /// <param name="data">寻址规则数据</param>
        /// <returns>资源的寻址地址</returns>
        string GetAssetAddress(AddressRuleData data);
    }
}
