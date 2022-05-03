
namespace YooAsset
{
	public class AssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 资源类型
		/// </summary>
		public System.Type AssetType { private set; get; }

		public AssetInfo(string assetPath, System.Type assetType)
		{
			AssetPath = assetPath;
			AssetType = assetType;
		}
		public AssetInfo(string assetPath)
		{
			AssetPath = assetPath;
			AssetType = null;
		}
	}
}