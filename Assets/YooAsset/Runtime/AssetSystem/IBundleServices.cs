
namespace YooAsset
{
	public interface IBundleServices
	{	
		/// <summary>
		/// 获取AssetBundle的信息
		/// </summary>
		AssetBundleInfo GetAssetBundleInfo(string bundleName);

		/// <summary>
		/// 获取资源所属的资源包名称
		/// </summary>
		string GetAssetBundleName(string assetPath);

		/// <summary>
		/// 获取资源依赖的所有AssetBundle列表
		/// </summary>
		string[] GetAllDependencies(string assetPath);
	}
}