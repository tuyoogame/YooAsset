
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
		/// bundle_name_000000000000000f000000000000000
		/// </summary>
		BundleName_HashName = 3,

		/// <summary>
		/// bundle_name_000000000000000f000000000000000.bundle
		/// </summary>
		BundleName_HashName_Extension = 4,
	}
}