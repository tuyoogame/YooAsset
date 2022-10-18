
namespace YooAsset
{
	internal class VerifyInfo
	{
		/// <summary>
		/// 验证的资源文件是否为内置资源
		/// </summary>
		public bool IsBuildinFile { private set; get; }

		/// <summary>
		/// 验证的资源包实例
		/// </summary>
		public PatchBundle VerifyBundle { private set; get; }

		/// <summary>
		/// 验证的文件路径
		/// </summary>
		public string VerifyFilePath { private set; get; }

		/// <summary>
		/// 验证结果
		/// </summary>
		public EVerifyResult Result;

		public VerifyInfo(bool isBuildinFile, PatchBundle verifyBundle)
		{
			IsBuildinFile = isBuildinFile;
			VerifyBundle = verifyBundle;
			VerifyFilePath = verifyBundle.CachedFilePath;
		}
	}
}