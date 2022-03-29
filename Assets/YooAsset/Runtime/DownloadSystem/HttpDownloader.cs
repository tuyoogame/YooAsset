using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;

namespace YooAsset
{
	internal sealed class HttpDownloader : DownloaderBase
	{
		/// <summary>
		/// 多线程下载器
		/// </summary>
		private class ThreadDownloader
		{
			private const int BufferSize = 1042 * 4;

			private Thread _thread;
			private string _url;
			private string _savePath;
			private string _fileHash;
			private string _fileCRC;
			private long _fileSize;
			private int _timeout;

			/// <summary>
			/// 下载是否结束
			/// </summary>
			public bool IsDone = false;

			/// <summary>
			/// 下载结果（成功或失败）
			/// </summary>
			public bool Result = true;

			/// <summary>
			/// 错误日志
			/// </summary>
			public string Error = string.Empty;

			/// <summary>
			/// 下载进度
			/// </summary>
			public float DownloadProgress = 0f;

			/// <summary>
			/// 已经下载的总字节数
			/// </summary>
			public ulong DownloadedBytes = 0;


			/// <summary>
			/// 开始下载
			/// </summary>
			public void Run(string url, string savePath, string fileHash, string fileCRC, long fileSize, int timeout)
			{
				_url = url;
				_savePath = savePath;
				_fileHash = fileHash;
				_fileCRC = fileCRC;
				_fileSize = fileSize;
				_timeout = timeout;

				_thread = new Thread(ThreadRun);
				_thread.IsBackground = true;
				_thread.Start();
			}

			/// <summary>
			/// 销毁下载器
			/// </summary>
			public void Dispose()
			{
				if (_thread != null)
				{
					_thread.Abort();
					_thread = null;
				}
			}


			private void ThreadRun()
			{
				long fileTotalSize = _fileSize;

				FileStream fileStream = null;
				Stream webStream = null;
				HttpWebResponse fileResponse = null;

				try
				{
					// 创建文件流
					fileStream = new FileStream(_savePath, FileMode.OpenOrCreate, FileAccess.Write);
					long fileLength = fileStream.Length - 1;

					// 创建HTTP下载请求
					HttpWebRequest fileRequest = WebRequest.Create(_url) as HttpWebRequest;
					fileRequest.Timeout = _timeout * 1000;
					fileRequest.ProtocolVersion = HttpVersion.Version10;
					if (fileLength > 0)
					{
						// 注意：设置远端请求文件的起始位置
						fileRequest.AddRange(fileLength);
						// 注意：设置本地文件流的起始位置
						fileStream.Seek(-1, SeekOrigin.End);
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
						float progress = fileLength / fileTotalSize;
						DownloadProgress = progress;
						DownloadedBytes = (ulong)fileLength;
					}
				}
				catch (Exception e)
				{
					Result = false;
					Error = e.Message;
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
						fileStream.Flush();
						fileStream.Close();
					}

					// 验证下载文件完整性
					if (Result)
					{
						bool verfiyResult = DownloadSystem.CheckContentIntegrity(_savePath, _fileSize, _fileCRC);
						if (verfiyResult == false)
						{
							Result = false;
							Error = $"Verify file content failed : {_fileHash}";
							if (File.Exists(_savePath))
								File.Delete(_savePath);
						}
					}

					IsDone = true;
				}
			}
		}


		private ThreadDownloader _threadDownloader;
		private float _tryAgainTimer;

		internal HttpDownloader(BundleInfo bundleInfo) : base(bundleInfo)
		{
		}
		internal override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			if (_steps == ESteps.CreateDownload)
			{
				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;
				_tryAgainTimer = 0f;

				_requestURL = GetRequestURL();
				_threadDownloader = new ThreadDownloader();
				_threadDownloader.Run(_requestURL, _bundleInfo.LocalPath, _bundleInfo.Hash, _bundleInfo.CRC, _bundleInfo.SizeBytes, _timeout);
				_steps = ESteps.CheckDownload;
			}

			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = _threadDownloader.DownloadProgress * 100f;
				_downloadedBytes = _threadDownloader.DownloadedBytes;
				if (_threadDownloader.IsDone == false)
					return;

				if (_threadDownloader.Result)
				{
					DownloadSystem.CacheVerifyFile(_bundleInfo.Hash, _bundleInfo.BundleName);
					_steps = ESteps.Succeed;
				}
				else
				{
					_lastError = _threadDownloader.Error;
					ReportError();

					// 失败后重新尝试
					if (_failedTryAgain > 0)
						_steps = ESteps.TryAgain;
					else
						_steps = ESteps.Failed;
				}

				// 释放下载器
				_threadDownloader.Dispose();
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				_tryAgainTimer += UnityEngine.Time.unscaledDeltaTime;
				if (_tryAgainTimer > 1f)
				{
					_failedTryAgain--;
					_steps = ESteps.CreateDownload;
					YooLogger.Warning($"Try again download : {_requestURL}");
				}
			}
		}
	}
}