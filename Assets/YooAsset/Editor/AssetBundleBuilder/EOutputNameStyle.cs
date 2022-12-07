
namespace YooAsset.Editor
{
	/// <summary>
	/// 输出文件名称的样式
	/// </summary>
	public enum EOutputNameStyle
	{
		/// <summary>
		/// 000000000000000f000000000000000
		/// </summary>
		HashName = 1,

		/// <summary>
		/// 000000000000000f000000000000000.bundle
		/// </summary>
		HashName_Extension = 2,

		/// <summary>
		/// 000000000000000f000000000000000_bundle_name.bundle
		/// </summary>
		HashName_BundleName_Extension = 4,
	}
}