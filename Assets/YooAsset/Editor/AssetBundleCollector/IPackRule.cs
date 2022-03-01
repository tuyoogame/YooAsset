
namespace YooAsset.Editor
{
	/// <summary>
	/// 资源打包规则接口
	/// </summary>
	public interface IPackRule
	{
		/// <summary>
		/// 获取资源的打包标签
		/// </summary>
		string GetAssetBundleLabel(string assetPath);
	}
}