using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
	internal static class CacheSystem
	{
		private readonly static Dictionary<string, PackageCache> _cachedDic = new Dictionary<string, PackageCache>(1000);

		/// <summary>
		/// 初始化时的验证级别
		/// </summary>
		public static EVerifyLevel InitVerifyLevel { set; get; } = EVerifyLevel.Low;

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
		{
			_cachedDic.Clear();
		}

		/// <summary>
		/// 查询是否为验证文件
		/// </summary>
		public static bool IsCached(string packageName, string cacheGUID)
		{
			var cache = GetOrCreateCache(packageName);
			return cache.IsCached(cacheGUID);
		}

		/// <summary>
		/// 录入验证的文件
		/// </summary>
		public static void RecordFile(string packageName, string cacheGUID, PackageCache.RecordWrapper wrapper)
		{
			var cache = GetOrCreateCache(packageName);
			cache.Record(cacheGUID, wrapper);
		}

		/// <summary>
		/// 丢弃验证的文件（同时删除文件）
		/// </summary>
		public static void DiscardFile(string packageName, string cacheGUID)
		{
			var cache = GetOrCreateCache(packageName);
			var wrapper = cache.TryGetWrapper(cacheGUID);
			if (wrapper == null)
				return;

			cache.Discard(cacheGUID);

			try
			{
				string dataFilePath = wrapper.DataFilePath;
				FileInfo fileInfo = new FileInfo(dataFilePath);
				if (fileInfo.Exists)
					fileInfo.Directory.Delete(true);
			}
			catch (Exception e)
			{
				YooLogger.Error($"Failed to delete cache file ! {e.Message}");
			}
		}

		/// <summary>
		/// 验证缓存文件（子线程内操作）
		/// </summary>
		public static EVerifyResult VerifyingCacheFile(VerifyElement element, EVerifyLevel verifyLevel)
		{
			try
			{
				string infoFilePath = element.InfoFilePath;
				if (File.Exists(infoFilePath) == false)
					return EVerifyResult.InfoFileNotExisted;

				// 解析信息文件获取验证数据
				string jsonContent = FileUtility.ReadAllText(infoFilePath);
				CacheFileInfo fileInfo = UnityEngine.JsonUtility.FromJson<CacheFileInfo>(jsonContent);
				element.DataFileCRC = fileInfo.FileCRC;
				element.DataFileSize = fileInfo.FileSize;
			}
			catch (Exception)
			{
				return EVerifyResult.Exception;
			}

			return VerifyingInternal(element.DataFilePath, element.DataFileSize, element.DataFileCRC, verifyLevel);
		}

		/// <summary>
		/// 验证下载文件
		/// </summary>
		public static EVerifyResult VerifyingTempFile(PatchBundle patchBundle, EVerifyLevel verifyLevel)
		{
			return VerifyingInternal(patchBundle.TempDataFilePath, patchBundle.FileSize, patchBundle.FileCRC, verifyLevel);
		}

		/// <summary>
		/// 验证记录文件
		/// </summary>
		public static EVerifyResult VerifyingRecordFile(string packageName, string cacheGUID)
		{
			var cache = GetOrCreateCache(packageName);
			var wrapper = cache.TryGetWrapper(cacheGUID);
			if (wrapper == null)
				return EVerifyResult.CacheNotFound;

			EVerifyResult result = VerifyingInternal(wrapper.DataFilePath, wrapper.DataFileSize, wrapper.DataFileCRC, EVerifyLevel.High);
			return result;
		}

		/// <summary>
		/// 获取未被使用的缓存文件
		/// </summary>
		public static List<string> GetUnusedCacheGUIDs(AssetsPackage package)
		{
			var cache = GetOrCreateCache(package.PackageName);
			var keys = cache.GetAllKeys();
			List<string> result = new List<string>(keys.Count);
			foreach (var cacheGUID in keys)
			{
				if (package.IsIncludeBundleFile(cacheGUID) == false)
				{
					result.Add(cacheGUID);
				}
			}
			return result;
		}


		private static EVerifyResult VerifyingInternal(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
		{
			try
			{
				if (File.Exists(filePath) == false)
					return EVerifyResult.DataFileNotExisted;

				// 先验证文件大小
				long size = FileUtility.GetFileSize(filePath);
				if (size < fileSize)
					return EVerifyResult.FileNotComplete;
				else if (size > fileSize)
					return EVerifyResult.FileOverflow;

				// 再验证文件CRC
				if (verifyLevel == EVerifyLevel.High)
				{
					string crc = HashUtility.FileCRC32(filePath);
					if (crc == fileCRC)
						return EVerifyResult.Succeed;
					else
						return EVerifyResult.FileCrcError;
				}
				else
				{
					return EVerifyResult.Succeed;
				}
			}
			catch (Exception)
			{
				return EVerifyResult.Exception;
			}
		}
		private static PackageCache GetOrCreateCache(string packageName)
		{
			if (_cachedDic.TryGetValue(packageName, out PackageCache cache) == false)
			{
				cache = new PackageCache(packageName);
				_cachedDic.Add(packageName, cache);
			}
			return cache;
		}
	}
}