
namespace YooAsset
{
	internal interface IBundleServices
	{
		/// <summary>
		/// 获取AssetBundle的信息
		/// </summary>
		BundleInfo GetBundleInfo(string bundleName);

		/// <summary>
		/// 获取资源所属的资源包名称
		/// </summary>
		string GetBundleName(string assetPath);

		/// <summary>
		/// 获取资源依赖的所有AssetBundle列表
		/// </summary>
		string[] GetAllDependencies(string assetPath);
	}
}