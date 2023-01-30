
namespace YooAsset.Editor
{
	public struct AddressRuleData
	{
		public string AssetPath;
		public string CollectPath;
		public string GroupName;
		public string Address;
		public bool IsMultiPlatform;

		public AddressRuleData(string assetPath, string collectPath, string groupName, string address, bool isMultiPlatform)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GroupName = groupName;
			Address = address;
			IsMultiPlatform = isMultiPlatform;
		}
	}

	/// <summary>
	/// 寻址规则接口
	/// </summary>
	public interface IAddressRule
	{
		string GetAssetAddress(AddressRuleData data);
	}
}