
namespace YooAsset
{
	public struct DecryptionFileInfo
	{
		public string BundleName;
		public string BundleHash;
		public string BundleCRC;
	}

	public interface IDecryptionServices
	{
		/// <summary>
		/// 获取加密文件的数据偏移量
		/// </summary>
		ulong GetFileOffset(DecryptionFileInfo fileInfo);
	}
}