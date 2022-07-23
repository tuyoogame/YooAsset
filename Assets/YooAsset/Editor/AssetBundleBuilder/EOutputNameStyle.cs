
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
		HashName,

		/// <summary>
		/// 000000000000000f000000000000000.bundle
		/// </summary>
		HashName_Extension,

		/// <summary>
		/// bundle_name_000000000000000f000000000000000
		/// </summary>
		BundleName_HashName,

		/// <summary>
		/// bundle_name_000000000000000f000000000000000.bundle
		/// </summary>
		BundleName_HashName_Extension,
	}
}