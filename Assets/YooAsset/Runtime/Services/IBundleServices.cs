
namespace YooAsset
{
	internal interface IBundleServices
	{
		/// <summary>
		/// 获取资源包信息
		/// </summary>
		BundleInfo GetBundleInfo(string bundleName);

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		AssetInfo[] GetAssetInfos(string[] tags);

		/// <summary>
		/// 映射为资源路径
		/// </summary>
		string MappingToAssetPath(string location);

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