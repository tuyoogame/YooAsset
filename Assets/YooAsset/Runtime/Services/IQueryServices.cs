
namespace YooAsset
{
	public interface IQueryServices
	{
		/// <summary>
		/// 查询应用程序里的内置资源是否存在
		/// </summary>
		bool QueryStreamingAssets(string packageName, string fileName);

		/// <summary>
		/// 查询开发者分发的文件加载路径
		/// </summary>
		string QueryDeliveryFiles(string packageName, string fileName);
	}
}