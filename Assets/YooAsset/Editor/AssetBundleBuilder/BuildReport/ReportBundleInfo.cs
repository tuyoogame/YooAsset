using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[Serializable]
	public class ReportBundleInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 哈希值
		/// </summary>
		public string Hash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string CRC;

		/// <summary>
		/// 文件大小（字节数）
		/// </summary>
		public long SizeBytes;

		/// <summary>
		/// Tags
		/// </summary>
		public string[] Tags;

		/// <summary>
		/// Flags
		/// </summary>
		public int Flags;

		public string GetTagsString()
		{
			if (Tags != null)
				return String.Join(";", Tags);
			else
				return string.Empty;
		}
	}
}