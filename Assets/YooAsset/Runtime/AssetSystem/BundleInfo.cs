
namespace YooAsset
{
	public class BundleInfo
	{
		private readonly PatchBundle _patchBundle;
		
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 本地存储的路径
		/// </summary>
		public string LocalPath { private set; get; }

		/// <summary>
		/// 远端下载地址
		/// </summary>
		public string RemoteMainURL { private set; get; }

		/// <summary>
		/// 远端下载备用地址
		/// </summary>
		public string RemoteFallbackURL { private set; get; }

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string Hash
		{
			get
			{
				if (_patchBundle == null)
					return string.Empty;
				else
					return _patchBundle.Hash;
			}
		}

		/// <summary>
		/// 校验的CRC
		/// </summary>
		public string CRC
		{
			get
			{
				if (_patchBundle == null)
					return string.Empty;
				else
					return _patchBundle.CRC;
			}
		}

		/// <summary>
		/// 文件大小
		/// </summary>
		public long SizeBytes
		{
			get
			{
				if (_patchBundle == null)
					return 0;
				else
					return _patchBundle.SizeBytes;
			}
		}

		/// <summary>
		/// 资源版本
		/// </summary>
		public int Version
		{
			get
			{
				if (_patchBundle == null)
					return 0;
				else
					return _patchBundle.Version;
			}
		}

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted
		{
			get
			{
				if (_patchBundle == null)
					return false;
				else
					return _patchBundle.IsEncrypted;
			}
		}

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile
		{
			get
			{
				if (_patchBundle == null)
					return false;
				else
					return _patchBundle.IsRawFile;
			}
		}


		private BundleInfo()
		{
		}
		internal BundleInfo(PatchBundle patchBundle, string localPath, string mainURL, string fallbackURL)
		{
			_patchBundle = patchBundle;
			BundleName = patchBundle.BundleName;
			LocalPath = localPath;
			RemoteMainURL = mainURL;
			RemoteFallbackURL = fallbackURL;
		}
		internal BundleInfo(PatchBundle patchBundle, string localPath)
		{
			_patchBundle = patchBundle;
			BundleName = patchBundle.BundleName;
			LocalPath = localPath;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
		}
		internal BundleInfo(string bundleName, string localPath)
		{
			_patchBundle = null;
			BundleName = bundleName;
			LocalPath = localPath;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
		}

		/// <summary>
		/// 是否为JAR包内文件
		/// </summary>
		public bool IsBuildinJarFile()
		{
			return LocalPath.StartsWith("jar:");
		}
	}
}