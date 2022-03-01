
namespace YooAsset.Editor
{
	public interface IAssetRedundancy
	{
		/// <summary>
		/// 检测是否冗余
		/// </summary>
		bool Check(string filePath);
	}
}