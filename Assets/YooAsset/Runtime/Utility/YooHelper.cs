using System.IO;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 资源路径帮助类
	/// </summary>
	internal static class PathHelper
	{
		private static string _buildinPath;
		private static string _sandboxPath;

		/// <summary>
		/// 获取基于流文件夹的加载路径
		/// </summary>
		public static string MakeStreamingLoadPath(string path)
		{
			if (string.IsNullOrEmpty(_buildinPath))
			{
				_buildinPath = StringUtility.Format("{0}/{1}", UnityEngine.Application.streamingAssetsPath, YooAssetSettings.StreamingAssetsBuildinFolder);
			}
			return StringUtility.Format("{0}/{1}", _buildinPath, path);
		}

		/// <summary>
		/// 获取基于沙盒文件夹的加载路径
		/// </summary>
		public static string MakePersistentLoadPath(string path)
		{
			string root = GetPersistentRootPath();
			return StringUtility.Format("{0}/{1}", root, path);
		}

		/// <summary>
		/// 获取沙盒文件夹路径
		/// </summary>
		public static string GetPersistentRootPath()
		{
#if UNITY_EDITOR
			// 注意：为了方便调试查看，编辑器下把存储目录放到项目里
			if (string.IsNullOrEmpty(_sandboxPath))
			{
				string directory = Path.GetDirectoryName(UnityEngine.Application.dataPath);
				string projectPath = GetRegularPath(directory);
				_sandboxPath = StringUtility.Format("{0}/Sandbox", projectPath);
			}
			return _sandboxPath;
#else
			if (string.IsNullOrEmpty(_sandboxPath))
			{
				_sandboxPath = StringUtility.Format("{0}/Sandbox", UnityEngine.Application.persistentDataPath);
			}
			return _sandboxPath;
#endif
		}
		private static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
		}

		/// <summary>
		/// 获取WWW加载本地资源的路径
		/// </summary>
		public static string ConvertToWWWPath(string path)
		{
#if UNITY_EDITOR
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_IPHONE
			return StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
			return path;
#elif UNITY_STANDALONE
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
			return path;
#endif
		}
	}

	/// <summary>
	/// 持久化目录帮助类
	/// </summary>
	internal static class PersistentHelper
	{
		private const string CacheFolderName = "CacheFiles";
		private const string CachedBundleFileFolder = "BundleFiles";
		private const string CachedRawFileFolder = "RawFiles";
		private const string ManifestFolderName = "ManifestFiles";
		private const string AppFootPrintFileName = "ApplicationFootPrint.bytes";


		/// <summary>
		/// 删除沙盒总目录
		/// </summary>
		public static void DeleteSandbox()
		{
			string directoryPath = PathHelper.MakePersistentLoadPath(string.Empty);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// 删除沙盒内的缓存文件夹
		/// </summary>
		public static void DeleteCacheFolder()
		{
			string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}

		/// <summary>
		/// 删除沙盒内的清单文件夹
		/// </summary>
		public static void DeleteManifestFolder()
		{
			string root = PathHelper.MakePersistentLoadPath(ManifestFolderName);
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}


		/// <summary>
		/// 获取缓存的BundleFile文件夹路径
		/// </summary>
		private readonly static Dictionary<string, string> _cachedBundleFileFolder = new Dictionary<string, string>(100);
		public static string GetCachedBundleFileFolderPath(string packageName)
		{
			if (_cachedBundleFileFolder.TryGetValue(packageName, out string value) == false)
			{
				string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
				value = $"{root}/{packageName}/{CachedBundleFileFolder}";
				_cachedBundleFileFolder.Add(packageName, value);
			}
			return value;
		}

		/// <summary>
		/// 获取缓存的RawFile文件夹路径
		/// </summary>
		private readonly static Dictionary<string, string> _cachedRawFileFolder = new Dictionary<string, string>(100);
		public static string GetCachedRawFileFolderPath(string packageName)
		{
			if (_cachedRawFileFolder.TryGetValue(packageName, out string value) == false)
			{
				string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
				value = $"{root}/{packageName}/{CachedRawFileFolder}";
				_cachedRawFileFolder.Add(packageName, value);
			}
			return value;
		}

		/// <summary>
		/// 获取应用程序的水印文件路径
		/// </summary>
		public static string GetAppFootPrintFilePath()
		{
			return PathHelper.MakePersistentLoadPath(AppFootPrintFileName);
		}

		/// <summary>
		/// 获取沙盒内清单文件的路径
		/// </summary>
		public static string GetCacheManifestFilePath(string packageName, string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetManifestBinaryFileName(packageName, packageVersion);
			return PathHelper.MakePersistentLoadPath($"{ManifestFolderName}/{fileName}");
		}

		/// <summary>
		/// 获取沙盒内包裹的哈希文件的路径
		/// </summary>
		public static string GetCachePackageHashFilePath(string packageName, string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetPackageHashFileName(packageName, packageVersion);
			return PathHelper.MakePersistentLoadPath($"{ManifestFolderName}/{fileName}");
		}

		/// <summary>
		/// 获取沙盒内包裹的版本文件的路径
		/// </summary>
		public static string GetCachePackageVersionFilePath(string packageName)
		{
			string fileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
			return PathHelper.MakePersistentLoadPath($"{ManifestFolderName}/{fileName}");
		}

		/// <summary>
		/// 保存默认的包裹版本
		/// </summary>
		public static void SaveCachePackageVersionFile(string packageName, string version)
		{
			string filePath = GetCachePackageVersionFilePath(packageName);
			FileUtility.CreateFile(filePath, version);
		}
	}
}