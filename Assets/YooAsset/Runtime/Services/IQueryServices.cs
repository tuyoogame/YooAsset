
namespace YooAsset
{
	/// <summary>
	/// 分发的资源信息
	/// </summary>
	public struct DeliveryFileInfo
	{
		public string DeliveryFilePath;
		public ulong DeliveryFileOffset;
	}

	public interface IQueryServices
	{
		/// <summary>
		/// 查询应用程序里的内置资源是否存在
		/// </summary>
		bool QueryStreamingAssets(string packageName, string fileName);

		/// <summary>
		/// 查询是否为开发者分发的资源
		/// </summary>
		bool QueryDeliveryFiles(string packageName, string fileName);

		/// <summary>
		/// 获取开发者分发的资源信息
		/// </summary>
		DeliveryFileInfo GetDeliveryFileInfo(string packageName, string fileName);
	}
}