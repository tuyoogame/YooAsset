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
		/// 获取缓存文件夹路径
		/// </summary>
		public static string GetCacheFolderPath(string packageName)
		{
			string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
			return $"{root}/{packageName}";
		}

		/// <summary>
		/// 获取应用程序的水印文件路径
		/// </summary>
		public static string GetAppFootPrintFilePath()
		{
			return PathHelper.MakePersistentLoadPath(AppFootPrintFileName);
		}

		#region 沙盒内清单相关
		/// <summary>
		/// 获取沙盒内清单文件的路径
		/// </summary>
		public static string GetCacheManifestFilePath(string packageName)
		{
			string fileName = YooAssetSettingsData.GetPatchManifestFileNameWithoutVersion(packageName);
			return PathHelper.MakePersistentLoadPath($"{ManifestFolderName}/{fileName}");
		}

		/// <summary>
		/// 加载沙盒内清单文件
		/// </summary>
		public static PatchManifest LoadCacheManifestFile(string packageName)
		{
			YooLogger.Log($"Load sandbox patch manifest file : {packageName}");
			string filePath = GetCacheManifestFilePath(packageName);
			byte[] bytesData = File.ReadAllBytes(filePath);
			return PatchManifest.DeserializeFromBinary(bytesData);
		}

		/// <summary>
		/// 存储沙盒内清单文件
		/// </summary>
		public static PatchManifest SaveCacheManifestFile(string packageName, byte[] fileBytesData)
		{
			YooLogger.Log($"Save sandbox patch manifest file : {packageName}");
			var manifest = PatchManifest.DeserializeFromBinary(fileBytesData);
			string savePath = GetCacheManifestFilePath(packageName);
			FileUtility.CreateFile(savePath, fileBytesData);
			return manifest;
		}

		/// <summary>
		/// 检测沙盒内清单文件是否存在
		/// </summary>
		public static bool CheckCacheManifestFileExists(string packageName)
		{
			string filePath = GetCacheManifestFilePath(packageName);
			return File.Exists(filePath);
		}

		/// <summary>
		/// 删除沙盒内清单文件
		/// </summary>
		public static bool DeleteCacheManifestFile(string packageName)
		{
			string filePath = GetCacheManifestFilePath(packageName);
			if (File.Exists(filePath))
			{
				YooLogger.Warning($"Invalid cache manifest file have been removed : {filePath}");
				File.Delete(filePath);
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion
	}
}