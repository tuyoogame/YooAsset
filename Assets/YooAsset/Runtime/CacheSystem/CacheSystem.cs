using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal static class CacheSystem
	{
		private readonly static HashSet<PatchBundle> _cacheBundles = new HashSet<PatchBundle>();
		private readonly static Dictionary<string, string> _cachedHashList = new Dictionary<string, string>(1000);
		private static EVerifyLevel _verifyLevel = EVerifyLevel.High;

		public static void Initialize(EVerifyLevel verifyLevel)
		{
			_verifyLevel = verifyLevel;
		}

		public static void DestroyAll()
		{
			_cacheBundles.Clear();
		}

		public static void WriteInfoFileForCachedFile()
		{

		}
		public static void ReadInfoFileForCachedFile()
		{

		}

		public static void GetCachingDiskSpaceUsed()
		{

		}
		public static void GetCachingDiskSpaceFree()
		{

		}

		public static bool IsCached(PatchBundle patchBundle)
		{
			return false;
		}
		public static void ClearCache()
		{

		}




		/// <summary>
		/// 查询是否为验证文件
		/// 注意：被收录的文件完整性是绝对有效的
		/// </summary>
		public static bool ContainsVerifyFile(string fileHash)
		{
			if (_cachedHashList.ContainsKey(fileHash))
			{
				string fileName = _cachedHashList[fileHash];
				string filePath = SandboxHelper.MakeCacheFilePath(fileName);
				if (File.Exists(filePath))
				{
					return true;
				}
				else
				{
					_cachedHashList.Remove(fileHash);
					YooLogger.Error($"Cache file is missing : {fileName}");
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 缓存验证过的文件
		/// </summary>
		public static void CacheVerifyFile(string fileHash, string fileName)
		{
			if (_cachedHashList.ContainsKey(fileHash) == false)
			{
				YooLogger.Log($"Cache verify file : {fileName}");
				_cachedHashList.Add(fileHash, fileName);
			}
		}

		/// <summary>
		/// 验证文件完整性
		/// </summary>
		public static bool CheckContentIntegrity(string filePath, long fileSize, string fileCRC)
		{
			return CheckContentIntegrity(_verifyLevel, filePath, fileSize, fileCRC);
		}

		/// <summary>
		/// 验证文件完整性
		/// </summary>
		public static bool CheckContentIntegrity(EVerifyLevel verifyLevel, string filePath, long fileSize, string fileCRC)
		{
			try
			{
				if (File.Exists(filePath) == false)
					return false;

				// 先验证文件大小
				long size = FileUtility.GetFileSize(filePath);
				if (size != fileSize)
					return false;

				// 再验证文件CRC
				if (verifyLevel == EVerifyLevel.High)
				{
					string crc = HashUtility.FileCRC32(filePath);
					return crc == fileCRC;
				}
				else
				{
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}