
namespace YooAsset
{
	/// <summary>
	/// 下载文件校验结果
	/// </summary>
	public enum EVerifyResult
	{
		/// <summary>
		/// 文件不存在
		/// </summary>
		FileNotExisted = -4,

		/// <summary>
		/// 文件内容不足（小于正常大小）
		/// </summary>
		FileNotComplete = -3,
		
		/// <summary>
		/// 文件内容溢出（超过正常大小）
		/// </summary>
		FileOverflow = -2,
		
		/// <summary>
		/// 文件内容不匹配
		/// </summary>
		FileCrcError = -1,

		/// <summary>
		/// 验证异常
		/// </summary>
		Exception = 0,

		/// <summary>
		/// 验证成功
		/// </summary>
		Succeed = 1,
	}
}