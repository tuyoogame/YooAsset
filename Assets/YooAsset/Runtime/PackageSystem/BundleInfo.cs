
namespace YooAsset
{
	internal class BundleInfo
	{
		public enum ELoadMode
		{
			None,
			LoadFromDelivery,
			LoadFromStreaming,
			LoadFromCache,
			LoadFromRemote,
			LoadFromEditor,
		}

		public readonly PackageBundle Bundle;
		public readonly ELoadMode LoadMode;

		/// <summary>
		/// 远端下载地址
		/// </summary>
		public string RemoteMainURL { private set; get; }

		/// <summary>
		/// 远端下载备用地址
		/// </summary>
		public string RemoteFallbackURL { private set; get; }

		/// <summary>
		/// 开发者分发的文件地址
		/// </summary>
		public string DeliveryFilePath { private set; get; }

		/// <summary>
		/// 开发者分发的文件偏移量
		/// </summary>
		public ulong DeliveryFileOffset { private set; get; }

		/// <summary>
		/// 注意：该字段只用于帮助编辑器下的模拟模式。
		/// </summary>
		public string[] IncludeAssets;


		private BundleInfo()
		{
		}
		public BundleInfo(PackageBundle bundle, ELoadMode loadMode, string mainURL, string fallbackURL)
		{
			Bundle = bundle;
			LoadMode = loadMode;
			RemoteMainURL = mainURL;
			RemoteFallbackURL = fallbackURL;
			DeliveryFilePath = string.Empty;
			DeliveryFileOffset = 0;
		}
		public BundleInfo(PackageBundle bundle, ELoadMode loadMode, string deliveryFilePath, ulong deliveryFileOffset)
		{
			Bundle = bundle;
			LoadMode = loadMode;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			DeliveryFilePath = deliveryFilePath;
			DeliveryFileOffset = deliveryFileOffset;
		}
		public BundleInfo(PackageBundle bundle, ELoadMode loadMode)
		{
			Bundle = bundle;
			LoadMode = loadMode;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			DeliveryFilePath = string.Empty;
			DeliveryFileOffset = 0;
		}

		/// <summary>
		/// 是否为JAR包内文件
		/// </summary>
		public static bool IsBuildinJarFile(string streamingPath)
		{
			return streamingPath.StartsWith("jar:");
		}
	}
}