using System.IO;
using System.Text;

namespace YooAsset
{
	internal static class PatchHelper
	{
		private const string StrCacheFileName = "Cache.bytes";
		private const string StrCacheFolderName = "CacheFiles";

		/// <summary>
		/// 清空沙盒目录
		/// </summary>
		public static void ClearSandbox()
		{
			string directoryPath = AssetPathHelper.MakePersistentLoadPath(string.Empty);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// 删除沙盒内补丁清单文件
		/// </summary>
		public static void DeleteSandboxPatchManifestFile()
		{
			string filePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
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
			string directoryPath = AssetPathHelper.MakePersistentLoadPath(StrCacheFolderName);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}


		/// <summary>
		/// 获取沙盒内缓存文件的路径
		/// </summary>
		public static string GetSandboxCacheFilePath()
		{
			return AssetPathHelper.MakePersistentLoadPath(StrCacheFileName);
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
			string filePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
			return File.Exists(filePath);
		}

		/// <summary>
		/// 获取沙盒内补丁清单文件的哈希值
		/// 注意：如果沙盒内补丁清单文件不存在，返回空字符串
		/// </summary>
		/// <returns></returns>
		public static string GetSandboxPatchManifestFileHash()
		{
			string filePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
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
			return AssetPathHelper.MakePersistentLoadPath($"{StrCacheFolderName}/{fileName}");
		}
	}
}