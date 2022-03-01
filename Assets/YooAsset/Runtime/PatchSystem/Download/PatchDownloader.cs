using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 补丁下载器
	/// </summary>
	public class PatchDownloader : IEnumerator
	{
		private const int MAX_LOADER_COUNT = 64;

		public delegate void OnDownloadOver(bool isSucceed);
		public delegate void OnDownloadProgress(int totalDownloadCount, int currentDownloadCoun, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnPatchFileDownloadFailed(string fileName);

		private readonly HostPlayModeImpl _playModeImpl;
		private readonly int _fileLoadingMaxNumber;
		private readonly int _failedTryAgain;
		private readonly List<AssetBundleInfo> _downloadList;
		private readonly List<AssetBundleInfo> _loadFailedList = new List<AssetBundleInfo>();
		private readonly List<FileDownloader> _downloaders = new List<FileDownloader>();
		private readonly List<FileDownloader> _removeList = new List<FileDownloader>(MAX_LOADER_COUNT);

		// 数据相关
		public EDownloaderStates DownloadStates { private set; get; }
		public int TotalDownloadCount { private set; get; }
		public long TotalDownloadBytes { private set; get; }
		public int CurrentDownloadCount { private set; get; }
		public long CurrentDownloadBytes { private set; get; }
		private long _lastDownloadBytes = 0;
		private int _lastDownloadCount = 0;

		// 委托相关
		public OnDownloadOver OnDownloadOverCallback { set; get; }
		public OnDownloadProgress OnDownloadProgressCallback { set; get; }
		public OnPatchFileDownloadFailed OnPatchFileDownloadFailedCallback { set; get; }


		private PatchDownloader()
		{
		}
		internal PatchDownloader(HostPlayModeImpl playModeImpl, List<AssetBundleInfo> downloadList, int fileLoadingMaxNumber, int failedTryAgain)
		{
			_playModeImpl = playModeImpl;
			_downloadList = downloadList;
			_fileLoadingMaxNumber = UnityEngine.Mathf.Clamp(fileLoadingMaxNumber, 1, MAX_LOADER_COUNT); ;
			_failedTryAgain = failedTryAgain;

			DownloadStates = EDownloaderStates.None;
			TotalDownloadCount = downloadList.Count;
			foreach (var patchBundle in downloadList)
			{
				TotalDownloadBytes += patchBundle.SizeBytes;
			}
		}

		/// <summary>
		/// 是否完毕，无论成功或失败
		/// </summary>
		public bool IsDone()
		{
			return DownloadStates == EDownloaderStates.Failed || DownloadStates == EDownloaderStates.Succeed;
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public void Download()
		{
			if (DownloadStates != EDownloaderStates.None)
			{
				Logger.Warning($"{nameof(PatchDownloader)} is already running.");
				return;
			}

			Logger.Log($"Begine to download : {TotalDownloadCount} files and {TotalDownloadBytes} bytes");
			DownloadStates = EDownloaderStates.Loading;
		}

		/// <summary>
		/// 更新下载器
		/// </summary>
		public void Update()
		{
			if (DownloadStates != EDownloaderStates.Loading)
				return;

			// 检测下载器结果
			_removeList.Clear();
			long downloadBytes = CurrentDownloadBytes;
			foreach (var downloader in _downloaders)
			{
				downloadBytes += (long)downloader.DownloadedBytes;
				if (downloader.IsDone() == false)
					continue;

				AssetBundleInfo bundleInfo = downloader.BundleInfo;

				// 检测是否下载失败
				if (downloader.HasError())
				{
					downloader.ReportError();
					_removeList.Add(downloader);
					_loadFailedList.Add(bundleInfo);
					continue;
				}

				// 下载成功
				_removeList.Add(downloader);
				CurrentDownloadCount++;
				CurrentDownloadBytes += bundleInfo.SizeBytes;
			}

			// 移除已经完成的下载器（无论成功或失败）
			foreach (var loader in _removeList)
			{
				_downloaders.Remove(loader);
			}

			// 如果下载进度发生变化
			if (_lastDownloadBytes != downloadBytes || _lastDownloadCount != CurrentDownloadCount)
			{
				_lastDownloadBytes = downloadBytes;
				_lastDownloadCount = CurrentDownloadCount;
				OnDownloadProgressCallback?.Invoke(TotalDownloadCount, _lastDownloadCount, TotalDownloadBytes, _lastDownloadBytes);
			}

			// 动态创建新的下载器到最大数量限制
			// 注意：如果期间有下载失败的文件，暂停动态创建下载器
			if (_downloadList.Count > 0 && _loadFailedList.Count == 0)
			{
				if (_downloaders.Count < _fileLoadingMaxNumber)
				{
					int index = _downloadList.Count - 1;
					var operation = DownloadSystem.BeginDownload(_downloadList[index], _failedTryAgain);
					_downloaders.Add(operation);
					_downloadList.RemoveAt(index);
				}
			}

			// 下载结算
			if (_downloaders.Count == 0)
			{
				if (_loadFailedList.Count > 0)
				{
					DownloadStates = EDownloaderStates.Failed;
					OnPatchFileDownloadFailedCallback?.Invoke(_loadFailedList[0].BundleName);
					OnDownloadOverCallback?.Invoke(false);
				}
				else
				{
					// 结算成功
					DownloadStates = EDownloaderStates.Succeed;
					OnDownloadOverCallback?.Invoke(true);
				}
			}
		}

		#region 异步相关
		bool IEnumerator.MoveNext()
		{
			return !IsDone();
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current
		{
			get { return null; }
		}
		#endregion
	}
}