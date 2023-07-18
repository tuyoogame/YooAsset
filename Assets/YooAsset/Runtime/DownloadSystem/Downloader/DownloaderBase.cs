using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	internal abstract class DownloaderBase
	{
		public enum EStatus
		{
			None = 0,
			Succeed,
			Failed
		}

		protected readonly BundleInfo _bundleInfo;
		protected readonly int _timeout;
		protected int _failedTryAgain;

		protected UnityWebRequest _webRequest;
		protected EStatus _status = EStatus.None;
		protected string _lastError = string.Empty;
		protected long _lastCode = 0;

		// 请求次数
		protected int _requestCount = 0;
		protected string _requestURL;

		// 下载进度
		protected float _downloadProgress = 0f;
		protected ulong _downloadedBytes = 0;

		// 超时相关
		protected bool _isAbort = false;
		protected ulong _latestDownloadBytes;
		protected float _latestDownloadRealtime;
		protected float _tryAgainTimer;

		/// <summary>
		/// 是否等待异步结束
		/// 警告：只能用于解压APP内部资源
		/// </summary>
		public bool WaitForAsyncComplete = false;

		/// <summary>
		/// 下载进度（0f~1f）
		/// </summary>
		public float DownloadProgress
		{
			get { return _downloadProgress; }
		}

		/// <summary>
		/// 已经下载的总字节数
		/// </summary>
		public ulong DownloadedBytes
		{
			get { return _downloadedBytes; }
		}


		public DownloaderBase(BundleInfo bundleInfo, int failedTryAgain, int timeout)
		{
			_bundleInfo = bundleInfo;
			_failedTryAgain = failedTryAgain;
			_timeout = timeout;
		}
		public abstract void SendRequest(params object[] param);
		public abstract void Update();
		public abstract void Abort();

		/// <summary>
		/// 获取下载文件的大小
		/// </summary>
		/// <returns></returns>
		public long GetDownloadFileSize()
		{
			return _bundleInfo.Bundle.FileSize;
		}

		/// <summary>
		/// 获取下载文件的资源包名
		/// </summary>
		public string GetDownloadBundleName()
		{
			return _bundleInfo.Bundle.BundleName;
		}

		/// <summary>
		/// 检测下载器是否已经完成（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			return _status == EStatus.Succeed || _status == EStatus.Failed;
		}

		/// <summary>
		/// 下载过程是否发生错误
		/// </summary>
		public bool HasError()
		{
			return _status == EStatus.Failed;
		}

		/// <summary>
		/// 按照错误级别打印错误
		/// </summary>
		public void ReportError()
		{
			YooLogger.Error(GetLastError());
		}

		/// <summary>
		/// 按照警告级别打印错误
		/// </summary>
		public void ReportWarning()
		{
			YooLogger.Warning(GetLastError());
		}

		/// <summary>
		/// 获取最近发生的错误信息
		/// </summary>
		public string GetLastError()
		{
			return $"Failed to download : {_requestURL} Error : {_lastError} Code : {_lastCode}";
		}


		/// <summary>
		/// 获取网络请求地址
		/// </summary>
		protected string GetRequestURL()
		{
			// 轮流返回请求地址
			_requestCount++;
			if (_requestCount % 2 == 0)
				return _bundleInfo.RemoteFallbackURL;
			else
				return _bundleInfo.RemoteMainURL;
		}

		/// <summary>
		/// 超时判定方法
		/// </summary>
		protected void CheckTimeout()
		{
			// 注意：在连续时间段内无新增下载数据及判定为超时
			if (_isAbort == false)
			{
				if (_latestDownloadBytes != DownloadedBytes)
				{
					_latestDownloadBytes = DownloadedBytes;
					_latestDownloadRealtime = Time.realtimeSinceStartup;
				}

				float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
				if (offset > _timeout)
				{
					YooLogger.Warning($"Web file request timeout : {_requestURL}");
					_webRequest.Abort();
					_isAbort = true;
				}
			}
		}

		/// <summary>
		/// 缓存下载文件
		/// </summary>
		protected void CachingFile(string tempFilePath)
		{
			string infoFilePath = _bundleInfo.Bundle.CachedInfoFilePath;
			string dataFilePath = _bundleInfo.Bundle.CachedDataFilePath;
			string dataFileCRC = _bundleInfo.Bundle.FileCRC;
			long dataFileSize = _bundleInfo.Bundle.FileSize;

			if (File.Exists(infoFilePath))
				File.Delete(infoFilePath);
			if (File.Exists(dataFilePath))
				File.Delete(dataFilePath);

			FileInfo fileInfo = new FileInfo(tempFilePath);
			fileInfo.MoveTo(dataFilePath);

			// 写入信息文件记录验证数据
			CacheFileInfo.WriteInfoToFile(infoFilePath, dataFileCRC, dataFileSize);

			// 记录缓存文件
			var wrapper = new PackageCache.RecordWrapper(infoFilePath, dataFilePath, dataFileCRC, dataFileSize);
			CacheSystem.RecordFile(_bundleInfo.Bundle.PackageName, _bundleInfo.Bundle.CacheGUID, wrapper);
		}
	}
}