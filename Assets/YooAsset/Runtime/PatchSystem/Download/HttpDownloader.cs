using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;

namespace YooAsset
{
	internal class HttpDownloader
	{
		private enum ESteps
		{
			None,
			CreateDownload,
			CheckDownload,
			Succeed,
			Failed,
		}

		private readonly BundleInfo _bundleInfo;
		private ESteps _steps = ESteps.None;

		// 线程
		private bool _threadOver = false;
		private bool _threadResult = false;
		private string _threadError = string.Empty;
		private Thread _thread;

		// 保留参数
		private int _timeout;
		private int _failedTryAgain;
		private int _requestCount;

		// 下载结果
		private string _downloadError = string.Empty;
		private float _downloadProgress = 0f;
		private long _downloadBytes = 0;

		/// <summary>
		/// 下载进度（0-100f）
		/// </summary>
		public float DownloadProgress 
		{ 
			get
			{
				return _downloadProgress;
			}
		}
		
		/// <summary>
		/// 已经下载的总字节数
		/// </summary>
		public long DownloadedBytes
		{
			get
			{
				return _downloadBytes;
			}
		}


		internal HttpDownloader(BundleInfo bundleInfo)
		{
			_bundleInfo = bundleInfo;
		}
		internal void SendRequest(int failedTryAgain, int timeout)
		{
			_failedTryAgain = failedTryAgain;
			_timeout = timeout;
		}
		internal void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (_steps == ESteps.Succeed || _steps == ESteps.Failed)
				return;

			if(_steps == ESteps.CreateDownload)
			{
				_downloadError = string.Empty;
				_downloadProgress = 0f;
				_downloadBytes = 0;

				_threadOver = false;
				_threadResult = false;
				_threadError = string.Empty;
				_thread = new Thread(ThreadRun);
				_thread.IsBackground = true;
				_thread.Start();
				_steps = ESteps.CheckDownload;
			}

			if(_steps == ESteps.CheckDownload)
			{
				if (_threadOver == false)
					return;

				if(_thread != null)
				{
					_thread.Abort();
					_thread = null;
				}

				_downloadError = _threadError;
				if (_threadResult)
				{
					DownloadSystem.CacheVerifyFile(_bundleInfo.Hash, _bundleInfo.BundleName);
					_steps = ESteps.Succeed;
				}
				else
				{
					// 失败后重新尝试
					if(_failedTryAgain > 0)
					{
						_failedTryAgain--;
						_steps = ESteps.CreateDownload;
					}
					else
					{
						_steps = ESteps.Failed;	
					}
				}
			}
		}
		internal void SetDone()
		{
			_steps = ESteps.Succeed;
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		public BundleInfo GetBundleInfo()
		{
			return _bundleInfo;
		}

		/// <summary>
		/// 检测下载器是否已经完成（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			return _steps == ESteps.Succeed || _steps == ESteps.Failed;
		}

		/// <summary>
		/// 下载过程是否发生错误
		/// </summary>
		public bool HasError()
		{
			return _steps == ESteps.Failed;
		}

		/// <summary>
		/// 报告错误信息
		/// </summary>
		public void ReportError()
		{
			Logger.Error(_downloadError);
		}


		#region 多线程下载
		public const int BufferSize = 1042 * 4;
		private void ThreadRun()
		{
			string url = GetRequestURL();
			string savePath = _bundleInfo.LocalPath;
			long fileTotalSize = _bundleInfo.SizeBytes;
			
			FileStream fileStream = null;
			Stream webStream = null;
			HttpWebResponse fileResponse = null;

			try
			{
				// 创建文件流
				fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
				long fileLength = fileStream.Length;

				// 创建HTTP下载请求
				HttpWebRequest fileRequest = WebRequest.Create(url) as HttpWebRequest;
				fileRequest.Timeout = _timeout;
				fileRequest.ReadWriteTimeout = _timeout;
				fileRequest.ProtocolVersion = HttpVersion.Version10;
				if (fileLength > 0)
				{
					// 注意：设置远端请求文件的起始位置
					fileRequest.AddRange(fileLength);
					// 注意：设置本地文件流的起始位置
					fileStream.Seek(fileLength, SeekOrigin.Begin);
				}

				// 读取下载数据并保存到文件
				fileResponse = fileRequest.GetResponse() as HttpWebResponse;
				webStream = fileResponse.GetResponseStream();
				byte[] buffer = new byte[BufferSize];
				while (true)
				{
					int length = webStream.Read(buffer, 0, buffer.Length);
					if (length <= 0)
						break;

					fileStream.Write(buffer, 0, length);

					// 计算下载进度
					// 注意：原子操作保证数据安全
					fileLength += length;
					float progress = (fileLength / fileTotalSize) * 100f;
					_downloadProgress = progress;
					_downloadBytes = fileLength;
				}

				// 验证下载文件完整性
				bool verfiyResult = DownloadSystem.CheckContentIntegrity(savePath, _bundleInfo.SizeBytes, _bundleInfo.CRC);
				if(verfiyResult)
				{
					_threadResult = true;
				}
				else
				{
					_threadResult = false;
					_threadError = $"Verify file content failed : {_bundleInfo.Hash}";
				}
			}
			catch (Exception e)
			{
				_threadResult = false;
				_threadError = e.Message;
			}
			finally
			{
				if (webStream != null)
				{
					webStream.Close();
					webStream.Dispose();
				}

				if (fileResponse != null)
				{
					fileResponse.Close();
				}

				if (fileStream != null)
				{
					fileStream.Close();
					fileStream.Dispose();
				}

				_threadOver = true;
			}
		}
		private string GetRequestURL()
		{
			// 轮流返回请求地址
			_requestCount++;
			if (_requestCount % 2 == 0)
				return _bundleInfo.RemoteFallbackURL;
			else
				return _bundleInfo.RemoteMainURL;
		}
		#endregion
	}
}