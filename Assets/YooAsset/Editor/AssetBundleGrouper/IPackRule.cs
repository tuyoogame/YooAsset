
namespace YooAsset.Editor
{
	/// <summary>
	/// 打包规则数据
	/// </summary>
	public struct PackRuleData
	{
		public string AssetPath;		
		public string CollectPath;
		public string GrouperName;

		public PackRuleData(string assetPath)
		{
			AssetPath = assetPath;
			CollectPath = string.Empty;
			GrouperName = string.Empty;
		}
		public PackRuleData(string assetPath, string collectPath, string grouperName)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GrouperName = grouperName;
		}
	}

	/// <summary>
	/// 资源打包规则接口
	/// </summary>
	public interface IPackRule
	{
		/// <summary>
		/// 获取资源打包所属的资源包名称
		/// </summary>
		string GetBundleName(PackRuleData data);
	}
}