
namespace YooAsset
{
	public interface IDeliveryQueryServices
	{
		/// <summary>
		/// 查询是否为开发者分发的资源文件
		/// </summary>
		bool Query(string packageName, string fileName);

		/// <summary>
		/// 获取分发资源文件的路径
		/// </summary>
		string GetFilePath(string packageName, string fileName);
	}
}