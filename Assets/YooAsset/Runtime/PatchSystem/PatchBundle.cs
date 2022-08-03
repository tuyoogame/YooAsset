using System;
using System.Linq;

namespace YooAsset
{
	[Serializable]
	internal class PatchBundle
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 内容哈希值
		/// </summary>
		public string ContentHash;

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileHash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string FileCRC;

		/// <summary>
		/// 文件大小（字节数）
		/// </summary>
		public long FileSize;

		/// <summary>
		/// 资源包的分类标签
		/// </summary>
		public string[] Tags;

		/// <summary>
		/// Flags
		/// </summary>
		public int Flags;


		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted { private set; get; }

		/// <summary>
		/// 是否为内置文件
		/// </summary>
		public bool IsBuildin { private set; get; }

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile { private set; get; }

		/// <summary>
		/// 文件名称
		/// </summary>	
		public string FileName { private set; get; }


		public PatchBundle(string bundleName, string contentHash, string fileHash, string fileCRC, long fileSize, string[] tags)
		{
			BundleName = bundleName;
			ContentHash = contentHash;
			FileHash = fileHash;
			FileCRC = fileCRC;
			FileSize = fileSize;
			Tags = tags;
		}

		/// <summary>
		/// 设置Flags
		/// </summary>
		public void SetFlagsValue(bool isEncrypted, bool isBuildin, bool isRawFile)
		{
			IsEncrypted = isEncrypted;
			IsBuildin = isBuildin;
			IsRawFile = isRawFile;

			BitMask32 mask = new BitMask32(0);
			if (isEncrypted) mask.Open(0);
			if (isBuildin) mask.Open(1);
			if (isRawFile) mask.Open(2);
			Flags = mask;
		}

		/// <summary>
		/// 解析Flags
		/// </summary>
		public void ParseFlagsValue()
		{
			BitMask32 value = Flags;
			IsEncrypted = value.Test(0);
			IsBuildin = value.Test(1);
			IsRawFile = value.Test(2);
		}

		/// <summary>
		/// 解析文件名称
		/// </summary>
		public void ParseFileName(int nameStype)
		{
			if (nameStype == 1)
			{
				FileName = FileHash;
			}
			else if (nameStype == 2)
			{
				string tempFileExtension = System.IO.Path.GetExtension(BundleName);
				FileName = $"{FileHash}{tempFileExtension}";
			}
			else if (nameStype == 3)
			{
				string tempFileExtension = System.IO.Path.GetExtension(BundleName);
				string tempBundleName = BundleName.Replace('/', '_').Replace(tempFileExtension, "");
				FileName = $"{tempBundleName}_{FileHash}";
			}
			else if (nameStype == 4)
			{
				string tempFileExtension = System.IO.Path.GetExtension(BundleName);
				string tempBundleName = BundleName.Replace('/', '_').Replace(tempFileExtension, "");
				FileName = $"{tempBundleName}_{FileHash}{tempFileExtension}";
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 是否包含Tag
		/// </summary>
		public bool HasTag(string[] tags)
		{
			if (tags == null || tags.Length == 0)
				return false;
			if (Tags == null || Tags.Length == 0)
				return false;

			foreach (var tag in tags)
			{
				if (Tags.Contains(tag))
					return true;
			}
			return false;
		}

		/// <summary>
		/// 是否为纯内置资源（不带任何Tag的资源）
		/// </summary>
		public bool IsPureBuildin()
		{
			if (Tags == null || Tags.Length == 0)
				return true;
			else
				return false;
		}
	}
}