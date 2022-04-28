
namespace YooAsset.Editor
{
	public struct AddressRuleData
	{
		public string AssetPath;
		public string CollectPath;
		public string GrouperName;

		public AddressRuleData(string assetPath, string collectPath, string grouperName)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GrouperName = grouperName;
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