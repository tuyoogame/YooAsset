
namespace YooAsset
{
	/// <summary>
	/// 文件加载器状态
	/// </summary>
	public enum ELoaderStates
	{
		None = 0,
		Download,
		CheckDownload,
		LoadFile,
		CheckFile,
		Success,
		Fail,
	}
}