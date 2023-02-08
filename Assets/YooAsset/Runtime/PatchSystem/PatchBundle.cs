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
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile;

		/// <summary>
		/// 加载方法
		/// </summary>
		public byte LoadMethod;

		/// <summary>
		/// 资源包的分类标签
		/// </summary>
		public string[] Tags;


		/// <summary>
		/// 所属的包裹名称
		/// </summary>
		public string PackageName { private set; get; }

		/// <summary>
		/// 缓存GUID
		/// </summary>
		public string CacheGUID
		{
			get { return FileHash; }
		}

		/// <summary>
		/// 缓存的数据文件路径
		/// </summary>
		private string _cachedDataFilePath;
		public string CachedDataFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(_cachedDataFilePath) == false)
					return _cachedDataFilePath;

				string cacheRoot = PersistentHelper.GetCacheFolderPath(PackageName);
				_cachedDataFilePath = $"{cacheRoot}/{CacheGUID}/{YooAssetSettings.CacheBundleDataFileName}";
				return _cachedDataFilePath;
			}
		}

		/// <summary>
		/// 缓存的信息文件路径
		/// </summary>
		private string _cachedInfoFilePath;
		public string CachedInfoFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(_cachedInfoFilePath) == false)
					return _cachedInfoFilePath;

				string cacheRoot = PersistentHelper.GetCacheFolderPath(PackageName);
				_cachedInfoFilePath = $"{cacheRoot}/{CacheGUID}/{YooAssetSettings.CacheBundleInfoFileName}";
				return _cachedInfoFilePath;
			}
		}

		/// <summary>
		/// 临时的数据文件路径
		/// </summary>
		private string _tempDataFilePath;
		public string TempDataFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(_tempDataFilePath) == false)
					return _tempDataFilePath;

				_tempDataFilePath = $"{CachedDataFilePath}.temp";
				return _tempDataFilePath;
			}
		}

		/// <summary>
		/// 内置文件路径
		/// </summary>
		private string _streamingFilePath;
		public string StreamingFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(_streamingFilePath) == false)
					return _streamingFilePath;

				_streamingFilePath = PathHelper.MakeStreamingLoadPath(FileName);
				return _streamingFilePath;
			}
		}

		/// <summary>
		/// 文件名称（远端文件名和内置文件名）
		/// </summary>
		private string _fileName;
		public string FileName
		{
			get
			{
				if (string.IsNullOrEmpty(_fileName))
					throw new Exception("Should never get here !");
				return _fileName;
			}
		}


		public PatchBundle()
		{
		}

		/// <summary>
		/// 解析资源包
		/// </summary>
		public void ParseBundle(string packageName, int nameStype)
		{
			PackageName = packageName;
			_fileName = PatchManifestTools.CreateBundleFileName(nameStype, BundleName, FileHash, IsRawFile);
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
		/// 是否包含任意Tags
		/// </summary>
		public bool HasAnyTags()
		{
			if (Tags != null && Tags.Length > 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 检测资源包文件内容是否相同
		/// </summary>
		public bool Equals(PatchBundle otherBundle)
		{
			if (FileHash == otherBundle.FileHash)
				return true;

			return false;
		}
	}
}