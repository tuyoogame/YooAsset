
namespace YooAsset
{
	/// <summary>
	/// 资源加载状态
	/// </summary>
	public enum EAssetStates
	{
		None = 0,
		CheckBundle,
		Loading,
		Checking,
		Success,
		Fail,
	}
}