
namespace YooAsset.Editor
{
	public interface IRedundancyServices
	{
		/// <summary>
		/// 检测是否冗余
		/// </summary>
		bool Check(string filePath);
	}
}