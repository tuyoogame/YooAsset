
namespace YooAsset
{
	internal class BundleInfo
	{
		public enum ELoadMode
		{
			None,
			LoadFromStreaming,
			LoadFromCache,
			LoadFromRemote,
			LoadFromEditor,
		}

		public readonly PatchBundle LoadBundle;
		public readonly ELoadMode LoadMode;

		private string _streamingPath;
		private string _cachePath;

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 文件名称
		/// </summary>
		public string FileName { private set; get; }

		/// <summary>
		/// 远端下载地址
		/// </summary>
		public string RemoteMainURL { private set; get; }

		/// <summary>
		/// 远端下载备用地址
		/// </summary>
		public string RemoteFallbackURL { private set; get; }

		/// <summary>
		/// 编辑器资源路径
		/// </summary>
		public string EditorAssetPath { private set; get; }

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileHash
		{
			get
			{
				if (LoadBundle == null)
					return string.Empty;
				else
					return LoadBundle.FileHash;
			}
		}
		
		/// <summary>
		/// 校验的CRC
		/// </summary>
		public string FileCRC
		{
			get
			{
				if (LoadBundle == null)
					return string.Empty;
				else
					return LoadBundle.FileCRC;
			}
		}

		/// <summary>
		/// 文件大小
		/// </summary>
		public long FileSize
		{
			get
			{
				if (LoadBundle == null)
					return 0;
				else
					return LoadBundle.FileSize;
			}
		}

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted
		{
			get
			{
				if (LoadBundle == null)
					return false;
				else
					return LoadBundle.IsEncrypted;
			}
		}

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile
		{
			get
			{
				if (LoadBundle == null)
					return false;
				else
					return LoadBundle.IsRawFile;
			}
		}

		/// <summary>
		/// 身份是否无效
		/// </summary>
		public bool IsInvalid
		{
			get
			{
				return LoadBundle == null;
			}
		}


		private BundleInfo()
		{
		}
		public BundleInfo(PatchBundle patchBundle, ELoadMode loadMode, string mainURL, string fallbackURL)
		{
			LoadBundle = patchBundle;
			LoadMode = loadMode;
			BundleName = patchBundle.BundleName;
			FileName = patchBundle.FileName;
			RemoteMainURL = mainURL;
			RemoteFallbackURL = fallbackURL;
			EditorAssetPath = string.Empty;
		}
		public BundleInfo(PatchBundle patchBundle, ELoadMode loadMode, string editorAssetPath)
		{
			LoadBundle = patchBundle;
			LoadMode = loadMode;
			BundleName = patchBundle.BundleName;
			FileName = patchBundle.FileName;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = editorAssetPath;
		}
		public BundleInfo(PatchBundle patchBundle, ELoadMode loadMode)
		{
			LoadBundle = patchBundle;
			LoadMode = loadMode;
			BundleName = patchBundle.BundleName;
			FileName = patchBundle.FileName;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = string.Empty;
		}

		/// <summary>
		/// 获取流文件夹的加载路径
		/// </summary>
		public string GetStreamingLoadPath()
		{
			if (LoadBundle == null)
				return string.Empty;

			if (string.IsNullOrEmpty(_streamingPath))
				_streamingPath = PathHelper.MakeStreamingLoadPath(LoadBundle.FileName);
			return _streamingPath;
		}

		/// <summary>
		/// 获取缓存文件夹的加载路径
		/// </summary>
		public string GetCacheLoadPath()
		{
			if (LoadBundle == null)
				return string.Empty;

			if (string.IsNullOrEmpty(_cachePath))
				_cachePath = SandboxHelper.MakeCacheFilePath(LoadBundle.FileName);
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