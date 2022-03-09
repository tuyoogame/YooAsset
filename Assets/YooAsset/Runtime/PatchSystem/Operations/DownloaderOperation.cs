using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	public class DownloaderOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Loading,
			Done,
		}

		private const int MAX_LOADER_COUNT = 64;

		public delegate void OnDownloadOver(bool isSucceed);
		public delegate void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnDownloadFileFailed(string fileName);
		
		private readonly int _fileLoadingMaxNumber;
		private readonly int _failedTryAgain;
		private readonly List<BundleInfo> _downloadList;
		private readonly List<BundleInfo> _loadFailedList = new List<BundleInfo>();
		private readonly List<FileDownloader> _downloaders = new List<FileDownloader>();
		private readonly List<FileDownloader> _removeList = new List<FileDownloader>(MAX_LOADER_COUNT);

		// 数据相关
		private ESteps _steps = ESteps.None;
		private long _lastDownloadBytes = 0;
		private int _lastDownloadCount = 0;


		/// <summary>
		/// 下载文件总数量
		/// </summary>
		public int TotalDownloadCount { private set; get; }

		/// <summary>
		/// 下载文件的总大小
		/// </summary>
		public long TotalDownloadBytes { private set; get; }

		/// <summary>
		/// 当前已经完成的下载总数量
		/// </summary>
		public int CurrentDownloadCount { private set; get; }

		/// <summary>
		/// 当前已经完成的下载总大小
		/// </summary>
		public long CurrentDownloadBytes { private set; get; }

		/// <summary>
		/// 当下载器结束（无论成功或失败）
		/// </summary>
		public OnDownloadOver OnDownloadOverCallback { set; get; }

		/// <summary>
		/// 当下载进度变化
		/// </summary>
		public OnDownloadProgress OnDownloadProgressCallback { set; get; }

		/// <summary>
		/// 当文件下载失败
		/// </summary>
		public OnDownloadFileFailed OnDownloadFileFailedCallback { set; get; }


		internal DownloaderOperation(List<BundleInfo> downloadList, int fileLoadingMaxNumber, int failedTryAgain)
		{
			_downloadList = downloadList;
			_fileLoadingMaxNumber = UnityEngine.Mathf.Clamp(fileLoadingMaxNumber, 1, MAX_LOADER_COUNT); ;
			_failedTryAgain = failedTryAgain;

			TotalDownloadCount = downloadList.Count;
			foreach (var patchBundle in downloadList)
			{
				TotalDownloadBytes += patchBundle.SizeBytes;
			}
		}
		internal override void Start()
		{
			YooLogger.Log($"Begine to download : {TotalDownloadCount} files and {TotalDownloadBytes} bytes");
			_steps = ESteps.Loading;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.Loading)
			{
				// 检测下载器结果
				_removeList.Clear();
				long downloadBytes = CurrentDownloadBytes;
				foreach (var downloader in _downloaders)
				{
					downloadBytes += (long)downloader.DownloadedBytes;
					if (downloader.IsDone() == false)
						continue;

					BundleInfo bundleInfo = downloader.GetBundleInfo();

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
						string fileName = _loadFailedList[0].BundleName;
						Error = $"Failed to download file : {fileName}";
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						OnDownloadFileFailedCallback?.Invoke(fileName);
						OnDownloadOverCallback?.Invoke(false);
					}
					else
					{
						// 结算成功
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
						OnDownloadOverCallback?.Invoke(true);
					}
				}
			}
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public void BeginDownload()
		{
			if (_steps == ESteps.None)
			{
				OperationSystem.ProcessOperaiton(this);
			}
		}
	}
}