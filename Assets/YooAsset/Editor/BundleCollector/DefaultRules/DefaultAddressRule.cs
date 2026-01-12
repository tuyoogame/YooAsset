using System.IO;

namespace YooAsset.Editor
{
    /// <summary>
    /// 禁用寻址
    /// </summary>
    [DisplayName("定位地址: 禁用")]
    public class AddressDisable : IAddressRule
    {
        /// <inheritdoc/>
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 以文件名作为寻址地址
    /// </summary>
    [DisplayName("定位地址: 文件名")]
    public class AddressByFileName : IAddressRule
    {
        /// <inheritdoc/>
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return Path.GetFileNameWithoutExtension(data.AssetPath);
        }
    }

    /// <summary>
    /// 以分组名和文件名作为寻址地址
    /// </summary>
    [DisplayName("定位地址: 分组名_文件名")]
    public class AddressByGroupAndFileName : IAddressRule
    {
        /// <inheritdoc/>
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            return $"{data.GroupName}_{fileName}";
        }
    }

    /// <summary>
    /// 以文件夹名和文件名作为寻址地址
    /// </summary>
    [DisplayName("定位地址: 文件夹名_文件名")]
    public class AddressByFolderAndFileName : IAddressRule
    {
        /// <inheritdoc/>
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            string directoryName = Path.GetFileName(Path.GetDirectoryName(data.AssetPath));
            return $"{directoryName}_{fileName}";
        }
    }
}
