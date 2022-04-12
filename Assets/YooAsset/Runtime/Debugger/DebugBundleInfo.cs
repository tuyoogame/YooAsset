
namespace YooAsset
{
	internal class DebugBundleInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { set; get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { set; get; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public AssetBundleLoaderBase.EStatus Status { set; get; }
	}
}