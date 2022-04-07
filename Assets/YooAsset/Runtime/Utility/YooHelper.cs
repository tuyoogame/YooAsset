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
			return StringUtility.Format("{0}/YooAssets/{1}", UnityEngine.Application.streamingAssetsPath, path);
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
		/// 获取网络资源加载路径
		/// </summary>
		public static string ConvertToWWWPath(string path)
		{
			// 注意：WWW加载方式，必须要在路径前面加file://
#if UNITY_EDITOR
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_IPHONE
			return StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
			return path;
#elif UNITY_STANDALONE
			return StringUtility.Format("file:///{0}", path);
#endif
		}

		/// <summary>
		/// 合并资源路径
		/// </summary>
		public static string CombineAssetPath(string root, string location)
		{
			if (string.IsNullOrEmpty(root))
				return location;
			else
				return $"{root}/{location}";
		}

		/// <summary>
		/// 获取AssetDatabase的加载路径
		/// </summary>
		public static string FindDatabaseAssetPath(string filePath)
		{
#if UNITY_EDITOR
			if (File.Exists(filePath))
				return filePath;

			// AssetDatabase加载资源需要提供文件后缀格式，然而资源定位地址并没有文件格式信息。
			// 所以我们通过查找该文件所在文件夹内同名的首个文件来确定AssetDatabase的加载路径。
			// 注意：AssetDatabase.FindAssets() 返回文件内包括递归文件夹内所有资源的GUID
			string fileName = Path.GetFileName(filePath);
			string directory = GetDirectory(filePath);
			string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Empty, new[] { directory });
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);

				if (UnityEditor.AssetDatabase.IsValidFolder(assetPath))
					continue;

				string assetDirectory = GetDirectory(assetPath);
				if (assetDirectory != directory)
					continue;

				string assetName = Path.GetFileNameWithoutExtension(assetPath);
				if (assetName == fileName)
					return assetPath;
			}

			// 没有找到同名的资源文件
			YooLogger.Warning($"Not found asset : {filePath}");
			return filePath;
#else
			throw new System.NotImplementedException();
#endif
		}
	}

	/// <summary>
	/// 沙盒帮助类
	/// </summary>
	internal static class SandboxHelper
	{
		private const string StrCacheFileName = "Cache.bytes";
		private const string StrCacheFolderName = "CacheFiles";

		/// <summary>
		/// 清空沙盒目录
		/// </summary>
		public static void ClearSandbox()
		{
			string directoryPath = PathHelper.MakePersistentLoadPath(string.Empty);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// 删除沙盒内补丁清单文件
		/// </summary>
		public static void DeleteSandboxPatchManifestFile()
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.Setting.PatchManifestFileName);
			if (File.Exists(filePath))
				File.Delete(filePath);
		}

		/// <summary>
		/// 删除沙盒内的缓存文件
		/// </summary>
		public static void DeleteSandboxCacheFile()
		{
			string filePath = GetSandboxCacheFilePath();
			if (File.Exists(filePath))
				File.Delete(filePath);
		}

		/// <summary>
		/// 删除沙盒内的缓存文件夹
		/// </summary>
		public static void DeleteSandboxCacheFolder()
		{
			string directoryPath = PathHelper.MakePersistentLoadPath(StrCacheFolderName);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}


		/// <summary>
		/// 获取沙盒内缓存文件的路径
		/// </summary>
		public static string GetSandboxCacheFilePath()
		{
			return PathHelper.MakePersistentLoadPath(StrCacheFileName);
		}

		/// <summary>
		/// 检测沙盒内缓存文件是否存在
		/// </summary>
		public static bool CheckSandboxCacheFileExist()
		{
			string filePath = GetSandboxCacheFilePath();
			return File.Exists(filePath);
		}

		/// <summary>
		/// 检测沙盒内补丁清单文件是否存在
		/// </summary>
		public static bool CheckSandboxPatchManifestFileExist()
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.Setting.PatchManifestFileName);
			return File.Exists(filePath);
		}

		/// <summary>
		/// 获取沙盒内补丁清单文件的哈希值
		/// 注意：如果沙盒内补丁清单文件不存在，返回空字符串
		/// </summary>
		/// <returns></returns>
		public static string GetSandboxPatchManifestFileHash()
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.Setting.PatchManifestFileName);
			if (File.Exists(filePath))
				return HashUtility.FileMD5(filePath);
			else
				return string.Empty;
		}

		/// <summary>
		/// 获取缓存文件的存储路径
		/// </summary>
		public static string MakeSandboxCacheFilePath(string fileName)
		{
			return PathHelper.MakePersistentLoadPath($"{StrCacheFolderName}/{fileName}");
		}
	}

	/// <summary>
	/// 补丁包帮助类
	/// </summary>
	internal static class PatchHelper
	{
		/// <summary>
		/// 获取内置资源解压列表
		/// </summary>
		public static List<BundleInfo> GetUnpackListByTags(PatchManifest appPatchManifest, string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in appPatchManifest.BundleList)
			{
				// 如果已经在沙盒内
				string filePath = SandboxHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
				if (System.IO.File.Exists(filePath))
					continue;

				// 如果不是内置资源
				if (patchBundle.IsBuildin == false)
					continue;

				// 如果是纯内置资源
				if (patchBundle.IsPureBuildin())
				{
					downloadList.Add(patchBundle);
				}
				else
				{
					// 查询DLC资源
					if (patchBundle.HasTag(tags))
					{
						downloadList.Add(patchBundle);
					}
				}
			}

			return ConvertToUnpackList(downloadList);
		}
		private static List<BundleInfo> ConvertToUnpackList(List<PatchBundle> unpackList)
		{
			List<BundleInfo> result = new List<BundleInfo>(unpackList.Count);
			foreach (var patchBundle in unpackList)
			{
				var bundleInfo = ConvertToUnpackInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result;
		}
		private static BundleInfo ConvertToUnpackInfo(PatchBundle patchBundle)
		{
			string sandboxPath = SandboxHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
			string streamingLoadPath = PathHelper.MakeStreamingLoadPath(patchBundle.Hash);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, sandboxPath, streamingLoadPath, streamingLoadPath);
			return bundleInfo;
		}
	}
}