
namespace YooAsset
{
	public interface IBuildinQueryServices
	{
		/// <summary>
		/// 查询应用程序里的内置资源是否存在
		/// </summary>
		bool QueryStreamingAssets(string packageName, string fileName);
	}
}