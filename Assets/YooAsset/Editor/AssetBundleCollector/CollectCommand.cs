
namespace YooAsset.Editor
{
	public class CollectCommand
	{
		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode { private set; get; }

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable { private set; get; }

		public CollectCommand(EBuildMode buildMode, bool enableAddressable)
		{
			BuildMode = buildMode;
			EnableAddressable = enableAddressable;
		}
	}
}