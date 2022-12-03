using System.IO;

namespace YooAsset.Editor
{
	[DisplayName("以文件名称为定位地址")]
	public class AddressByFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			return Path.GetFileNameWithoutExtension(data.AssetPath);
		}
	}

	[DisplayName("以分组名称+文件名称为定位地址")]
	public class AddressByGroupAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			return $"{data.GroupName}_{fileName}";
		}
	}

	[DisplayName("以收集器名称+文件名称为定位地址")]
	public class AddressByCollectorAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			string collectorName = Path.GetFileNameWithoutExtension(data.CollectPath);
			return $"{collectorName}_{fileName}";
		}
	}
}