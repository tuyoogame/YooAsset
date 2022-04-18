using System.IO;

namespace YooAsset
{
	public class RawFileOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Prepare,
			DownloadFromWeb,
			CheckDownloadFromWeb,
			DownloadFromApk,
			CheckDownloadFromApk,
			CheckAndCopyFile,
			Done,
		}

		private readonly BundleInfo _bundleInfo;
		private ESteps _steps = ESteps.None;
		private DownloaderBase _downloader;
		private UnityWebFileRequester _fileRequester;

		/// <summary>
		/// 原生文件的拷贝路径
		/// </summary>
		public string CopyPath { private set; get; }

		/// <summary>
		/// 原生文件的缓存路径
		/// </summary>
		public string CachePath
		{
			get
			{
				if (_bundleInfo == null)
					return string.Empty;
				return _bundleInfo.GetCacheLoadPath();
			}
		}


		internal RawFileOperation(BundleInfo bundleInfo, string copyPath)
		{
			_bundleInfo = bundleInfo;
			CopyPath = copyPath;
		}
		internal override void Start()
		{
			_steps = ESteps.Prepare;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			// 1. 准备工作
			if (_steps == ESteps.Prepare)
			{
				if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.None)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Bundle info is invalid : {_bundleInfo.BundleName}";
					return;
				}

				if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				{
					_steps = ESteps.DownloadFromWeb;
				}
				else if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.DownloadFromApk;
				}
				else if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
				{
					_steps = ESteps.CheckAndCopyFile;
				}
				else
				{
					throw new System.NotImplementedException(_bundleInfo.LoadMode.ToString());
				}
			}

			// 2. 从服务器下载
			if (_steps == ESteps.DownloadFromWeb)
			{
				int failedTryAgain = int.MaxValue;
				_downloader = DownloadSystem.BeginDownload(_bundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownloadFromWeb;
			}

			// 3. 检测服务器下载结果
			if (_steps == ESteps.CheckDownloadFromWeb)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetLastError();
				}
				else
				{
					_steps = ESteps.CheckAndCopyFile;
				}
			}

			// 4. 从APK拷贝文件
			if (_steps == ESteps.DownloadFromApk)
			{
				string downloadURL = PathHelper.ConvertToWWWPath(_bundleInfo.GetStreamingLoadPath());
				_fileRequester = new UnityWebFileRequester();
				_fileRequester.SendRequest(downloadURL, _bundleInfo.GetCacheLoadPath());
				_steps = ESteps.CheckDownloadFromApk;
			}

			// 5. 检测APK拷贝文件结果
			if (_steps == ESteps.CheckDownloadFromApk)
			{
				if (_fileRequester.IsDone() == false)
					return;

				if (_fileRequester.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _fileRequester.GetError();
				}
				else
				{
					_steps = ESteps.CheckAndCopyFile;
				}
				_fileRequester.Dispose();
			}

			// 6. 检测并拷贝原生文件
			if (_steps == ESteps.CheckAndCopyFile)
			{
				// 如果不需要保存文件
				if (string.IsNullOrEmpty(CopyPath))
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					return;
				}

				// 如果原生文件已经存在，则验证其完整性
				if (File.Exists(CopyPath))
				{
					bool result = DownloadSystem.CheckContentIntegrity(CopyPath, _bundleInfo.SizeBytes, _bundleInfo.CRC);
					if (result)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
						return;
					}
					else
					{
						File.Delete(CopyPath);
					}
				}

				try
				{
					FileUtility.CreateFileDirectory(CopyPath);
					File.Copy(_bundleInfo.GetCacheLoadPath(), CopyPath, true);
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				catch (System.Exception e)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = e.ToString();
				}
			}
		}

		/// <summary>
		/// 获取原生文件的二进制数据
		/// </summary>
		public byte[] GetFileData()
		{
			string cachePath = _bundleInfo.GetCacheLoadPath();
			if (File.Exists(cachePath) == false)
				return null;
			return File.ReadAllBytes(cachePath);
		}

		/// <summary>
		/// 获取原生文件的文本数据
		/// </summary>
		public string GetFileText()
		{
			string cachePath = _bundleInfo.GetCacheLoadPath();
			if (File.Exists(cachePath) == false)
				return string.Empty;
			return File.ReadAllText(cachePath, System.Text.Encoding.UTF8);
		}
	}
}