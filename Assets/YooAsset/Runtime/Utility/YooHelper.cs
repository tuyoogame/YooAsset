using System.IO;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 资源路径帮助类
	/// </summary>
	internal static class PathHelper
	{
		/// <summary>
		/// 获取规范化的路径
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
		}

		/// <summary>
		/// 获取文件所在的目录路径（Linux格式）
		/// </summary>
		public static string GetDirectory(string filePath)
		{
			string directory = Path.GetDirectoryName(filePath);
			return GetRegularPath(directory);
		}

		/// <summary>
		/// 获取基于流文件夹的加载路径
		/// </summary>
		public static string MakeStreamingLoadPath(string path)
		{
			return StringUtility.Format("{0}/{1}/{2}", UnityEngine.Application.streamingAssetsPath, YooAssetSettings.StreamingAssetsBuildinFolder, path);
		}

		/// <summary>
		/// 获取基于沙盒文件夹的加载路径
		/// </summary>
		public static string MakePersistentLoadPath(string path)
		{
			string root = MakePersistentRootPath();
			return StringUtility.Format("{0}/{1}", root, path);
		}

		/// <summary>
		/// 获取沙盒文件夹路径
		/// </summary>
		public static string MakePersistentRootPath()
		{
#if UNITY_EDITOR
			// 注意：为了方便调试查看，编辑器下把存储目录放到项目里
			string projectPath = GetDirectory(UnityEngine.Application.dataPath);
			return StringUtility.Format("{0}/Sandbox", projectPath);
#else
			return StringUtility.Format("{0}/Sandbox", UnityEngine.Application.persistentDataPath);
#endif
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
		/// 获取缓存文件夹路径
		/// </summary>
		public static string GetCacheFolderPath(string packageName)
		{
			string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
			return $"{root}/{packageName}";
		}

		#region 沙盒内清单相关
		/// <summary>
		/// 获取沙盒内清单文件的路径
		/// </summary>
		public static string GetCacheManifestFilePath(string packageName)
		{
			string fileName = YooAssetSettingsData.GetPatchManifestFileNameWithoutVersion(packageName);
			return PathHelper.MakePersistentLoadPath(fileName);
		}

		/// <summary>
		/// 加载沙盒内清单文件
		/// </summary>
		public static PatchManifest LoadCacheManifestFile(string packageName)
		{
			YooLogger.Log($"Load sandbox patch manifest file : {packageName}");
			string filePath = GetCacheManifestFilePath(packageName);
			string jsonData = File.ReadAllText(filePath);
			return PatchManifest.Deserialize(jsonData);
		}

		/// <summary>
		/// 存储沙盒内清单文件
		/// </summary>
		public static PatchManifest SaveCacheManifestFile(string packageName, string fileContent)
		{
			YooLogger.Log($"Save sandbox patch manifest file : {packageName}");
			var manifest = PatchManifest.Deserialize(fileContent);
			string savePath = GetCacheManifestFilePath(packageName);
			FileUtility.CreateFile(savePath, fileContent);
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