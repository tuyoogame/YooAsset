using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Networking;

namespace YooAsset
{
	/// <summary>
	/// 自定义下载器的请求委托
	/// </summary>
	public delegate UnityWebRequest DownloadRequestDelegate(string url);

	/// <summary>
	/// 1. 保证每一时刻资源文件只存在一个下载器
	/// 2. 保证下载器下载完成后立刻验证并缓存
	/// 3. 保证资源文件不会被重复下载
	/// </summary>
	internal static class DownloadSystem
	{
		private static readonly Dictionary<string, DownloaderBase> _downloaderDic = new Dictionary<string, DownloaderBase>();
		private static readonly List<string> _removeList = new List<string>(100);

		/// <summary>
		/// 自定义下载器的请求委托
		/// </summary>
		public static DownloadRequestDelegate RequestDelegate = null;

		/// <summary>
		/// 自定义的证书认证实例
		/// </summary>
		public static CertificateHandler CertificateHandlerInstance = null;

		/// <summary>
		/// 网络重定向次数
		/// </summary>
		public static int RedirectLimit { set; get; } = -1;

		/// <summary>
		/// 启用断点续传功能文件的最小字节数
		/// </summary>
		public static int BreakpointResumeFileSize { set; get; } = int.MaxValue;

		/// <summary>
		/// 下载失败后清理文件的HTTP错误码
		/// </summary>
		public static List<long> ClearFileResponseCodes { set; get; }


		/// <summary>
		/// 初始化下载器
		/// </summary>
		public static void Initialize()
		{
		}

		/// <summary>
		/// 更新下载器
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

			RequestDelegate = null;
			CertificateHandlerInstance = null;
			BreakpointResumeFileSize = int.MaxValue;
			ClearFileResponseCodes = null;
		}


		/// <summary>
		/// 创建下载器
		/// 注意：只有第一次请求的参数才有效
		/// </summary>
		public static DownloaderBase CreateDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
		{
			// 查询存在的下载器
			if (_downloaderDic.TryGetValue(bundleInfo.Bundle.CachedDataFilePath, out var downloader))
				return downloader;

			// 如果资源已经缓存
			if (CacheSystem.IsCached(bundleInfo.Bundle.PackageName, bundleInfo.Bundle.CacheGUID))
			{
				var completedDownloader = new CompletedDownloader(bundleInfo);
				return completedDownloader;
			}

			// 创建新的下载器	
			YooLogger.Log($"Beginning to download bundle : {bundleInfo.Bundle.BundleName} URL : {bundleInfo.RemoteMainURL}");
#if UNITY_WEBGL
			if (bundleInfo.Bundle.IsRawFile)
			{
				FileUtility.CreateFileDirectory(bundleInfo.Bundle.CachedDataFilePath);
				DownloaderBase newDownloader = new FileGeneralDownloader(bundleInfo, failedTryAgain, timeout);
				_downloaderDic.Add(bundleInfo.Bundle.CachedDataFilePath, newDownloader);
				return newDownloader;
			}
			else
			{
				WebDownloader newDownloader = new WebDownloader(bundleInfo, failedTryAgain, timeout);
				_downloaderDic.Add(bundleInfo.Bundle.CachedDataFilePath, newDownloader);
				return newDownloader;
			}
#else
			FileUtility.CreateFileDirectory(bundleInfo.Bundle.CachedDataFilePath);
			bool resumeDownload = bundleInfo.Bundle.FileSize >= BreakpointResumeFileSize;
			DownloaderBase newDownloader;
			if (resumeDownload)
				newDownloader = new FileResumeDownloader(bundleInfo, failedTryAgain, timeout);
			else
				newDownloader = new FileGeneralDownloader(bundleInfo, failedTryAgain, timeout);
			_downloaderDic.Add(bundleInfo.Bundle.CachedDataFilePath, newDownloader);
			return newDownloader;
#endif
		}

		/// <summary>
		/// 创建一个新的网络请求
		/// </summary>
		public static UnityWebRequest NewRequest(string requestURL)
		{
			UnityWebRequest webRequest;
			if (RequestDelegate != null)
				webRequest = RequestDelegate.Invoke(requestURL);
			else
				webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbGET);

			SetUnityWebRequestParam(webRequest);
			return webRequest;
		}

		/// <summary>
		/// 设置网络请求的自定义参数
		/// </summary>
		private static void SetUnityWebRequestParam(UnityWebRequest webRequest)
		{
			if (RedirectLimit >= 0)
				webRequest.redirectLimit = RedirectLimit;

			if (CertificateHandlerInstance != null)
			{
				webRequest.certificateHandler = CertificateHandlerInstance;
				webRequest.disposeCertificateHandlerOnDispose = false;
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