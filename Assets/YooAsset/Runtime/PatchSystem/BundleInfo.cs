
namespace YooAsset
{
	public class BundleInfo
	{
		internal enum ELoadMode
		{
			None,
			LoadFromStreaming,
			LoadFromCache,
			LoadFromRemote,
			LoadFromEditor,
		}

		private readonly PatchBundle _patchBundle;
		internal readonly ELoadMode LoadMode;

		private string _streamingPath;
		private string _cachePath;

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 远端下载地址
		/// </summary>
		internal string RemoteMainURL { private set; get; }

		/// <summary>
		/// 远端下载备用地址
		/// </summary>
		internal string RemoteFallbackURL { private set; get; }

		/// <summary>
		/// 编辑器资源路径
		/// </summary>
		public string EditorAssetPath { private set; get; }

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
		internal BundleInfo(PatchBundle patchBundle, ELoadMode loadMode, string mainURL, string fallbackURL)
		{
			_patchBundle = patchBundle;
			LoadMode = loadMode;
			BundleName = patchBundle.BundleName;
			RemoteMainURL = mainURL;
			RemoteFallbackURL = fallbackURL;
			EditorAssetPath = string.Empty;
		}
		internal BundleInfo(PatchBundle patchBundle, ELoadMode loadMode, string editorAssetPath)
		{
			_patchBundle = patchBundle;
			LoadMode = loadMode;
			BundleName = patchBundle.BundleName;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = editorAssetPath;
		}
		internal BundleInfo(PatchBundle patchBundle, ELoadMode loadMode)
		{
			_patchBundle = patchBundle;
			LoadMode = loadMode;
			BundleName = patchBundle.BundleName;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = string.Empty;
		}
		internal BundleInfo(string bundleName)
		{
			_patchBundle = null;
			LoadMode = ELoadMode.None;
			BundleName = bundleName;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = string.Empty;
		}

		/// <summary>
		/// 资源包是否有效
		/// </summary>
		public bool IsValid()
		{
			return _patchBundle != null;
		}

		/// <summary>
		/// 资源包文件是否在云端
		/// </summary>
		public bool InCloud()
		{
			return LoadMode == ELoadMode.LoadFromRemote;
		}

		/// <summary>
		/// 获取流文件夹的加载路径
		/// </summary>
		internal string GetStreamingLoadPath()
		{
			if (_patchBundle == null)
				return string.Empty;

			if (string.IsNullOrEmpty(_streamingPath))
				_streamingPath = PathHelper.MakeStreamingLoadPath(_patchBundle.Hash);
			return _streamingPath;
		}

		/// <summary>
		/// 获取缓存文件夹的加载路径
		/// </summary>
		internal string GetCacheLoadPath()
		{
			if (_patchBundle == null)
				return string.Empty;

			if (string.IsNullOrEmpty(_cachePath))
				_cachePath = SandboxHelper.MakeCacheFilePath(_patchBundle.Hash);
			return _cachePath;
		}


		/// <summary>
		/// 是否为JAR包内文件
		/// </summary>
		public static bool IsBuildinJarFile(string streamingPath)
		{
			return streamingPath.StartsWith("jar:");
		}
	}
}