
namespace YooAsset.Editor
{
	/// <summary>
	/// 资源过滤规则接口
	/// </summary>
	public interface IFilterRule
	{
		/// <summary>
		/// 是否为收集资源
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <returns>如果收集该资源返回TRUE</returns>
		bool IsCollectAsset(string assetPath);
	}
}