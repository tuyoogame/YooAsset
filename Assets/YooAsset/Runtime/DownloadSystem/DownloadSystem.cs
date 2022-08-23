using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 1. 保证每一时刻资源文件只存在一个下载器
	/// 2. 保证下载器下载完成后立刻验证并缓存
	/// 3. 保证资源文件不会被重复下载
	/// </summary>
	internal static class DownloadSystem
	{
		private static readonly Dictionary<string, DownloaderBase> _downloaderDic = new Dictionary<string, DownloaderBase>();
		private static readonly List<string> _removeList = new List<string>(100);
		private static int _breakpointResumeFileSize = int.MaxValue;


		/// <summary>
		/// 初始化
		/// </summary>
		public static void Initialize(int breakpointResumeFileSize)
		{
			_breakpointResumeFileSize = breakpointResumeFileSize;
		}

		/// <summary>
		/// 更新所有下载器
		/// </summary>
		public static void Update()
		{
			// 更新下载器
			_removeList.Clear();
			foreach (var valuePair in _downloaderDic)
			{
				var downloader = valuePair.Value;
				downloader.Update();
				if (downloader.IsDone())
					_removeList.Add(valuePair.Key);
			}

			// 移除下载器
			foreach (var key in _removeList)
			{
				_downloaderDic.Remove(key);
			}
		}

		/// <summary>
		/// 销毁所有下载器
		/// </summary>
		public static void DestroyAll()
		{
			foreach (var valuePair in _downloaderDic)
			{
				var downloader = valuePair.Value;
				downloader.Abort();
			}
			_downloaderDic.Clear();
			_removeList.Clear();
			_breakpointResumeFileSize = int.MaxValue;
		}


		/// <summary>
		/// 开始下载资源文件
		/// 注意：只有第一次请求的参数才是有效的
		/// </summary>
		public static DownloaderBase BeginDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
		{
			// 查询存在的下载器
			if (_downloaderDic.TryGetValue(bundleInfo.Bundle.CachedFilePath, out var downloader))
			{
				return downloader;
			}

			// 如果资源已经缓存
			if (CacheSystem.IsCached(bundleInfo.Bundle))
			{
				var tempDownloader = new TempDownloader(bundleInfo);
				return tempDownloader;
			}

			// 创建新的下载器	
			{
				YooLogger.Log($"Beginning to download file : {bundleInfo.Bundle.FileName} URL : {bundleInfo.RemoteMainURL}");
				FileUtility.CreateFileDirectory(bundleInfo.Bundle.CachedFilePath);
				bool breakDownload = bundleInfo.Bundle.FileSize >= _breakpointResumeFileSize;
				DownloaderBase newDownloader = new FileDownloader(bundleInfo, breakDownload);
				newDownloader.SendRequest(failedTryAgain, timeout);
				_downloaderDic.Add(bundleInfo.Bundle.CachedFilePath, newDownloader);
				return newDownloader;
			}
		}

		/// <summary>
		/// 获取下载器的总数
		/// </summary>
		public static int GetDownloaderTotalCount()
		{
			return _downloaderDic.Count;
		}
	}
}