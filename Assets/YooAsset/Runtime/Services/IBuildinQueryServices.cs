
namespace YooAsset
{
	public interface IBuildinQueryServices
	{
		/// <summary>
		/// 查询是否为应用程序内置的资源文件
		/// </summary>
		bool Query(string packageName, string fileName);
	}
}