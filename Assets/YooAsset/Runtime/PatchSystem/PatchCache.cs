using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset.Utility;

namespace YooAsset
{
	[Serializable]
	internal sealed class PatchCache
	{
		/// <summary>
		/// 缓存的APP内置版本
		/// </summary>
		public string CacheAppVersion = string.Empty;

		/// <summary>
		/// 读取缓存文件
		/// 注意：如果文件不存在则创建新的缓存文件
		/// </summary>
		public static PatchCache LoadCache()
		{
			if (PatchHelper.CheckSandboxCacheFileExist())
			{
				Logger.Log("Load patch cache from disk.");
				string filePath = PatchHelper.GetSandboxCacheFilePath();
				string jsonData = FileUtility.ReadFile(filePath);
				return JsonUtility.FromJson<PatchCache>(jsonData);
			}
			else
			{
				Logger.Log($"Create patch cache to disk : {Application.version}");
				PatchCache cache = new PatchCache();
				cache.CacheAppVersion = Application.version;
				string filePath = PatchHelper.GetSandboxCacheFilePath();
				string jsonData = JsonUtility.ToJson(cache);
				FileUtility.CreateFile(filePath, jsonData);
				return cache;
			}
		}

		/// <summary>
		/// 更新缓存文件
		/// </summary>
		public static void UpdateCache()
		{
			Logger.Log($"Update patch cache to disk : {Application.version}");
			PatchCache cache = new PatchCache();
			cache.CacheAppVersion = Application.version;
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = JsonUtility.ToJson(cache);
			FileUtility.CreateFile(filePath, jsonData);
		}
	}
}