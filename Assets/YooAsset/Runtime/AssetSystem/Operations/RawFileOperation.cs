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
			CheckFile,
			DownloadFromApk,
			CheckDownloadFromApk,
			Done,
		}

		private readonly BundleInfo _bundleInfo;
		private readonly string _savePath;
		private ESteps _steps = ESteps.None;
		private DownloaderBase _downloader;
		private UnityWebFileRequester _fileRequester;

		/// <summary>
		/// 原生文件的存储路径
		/// </summary>
		public string SavePath
		{
			get { return _savePath; }
		}


		internal RawFileOperation(BundleInfo bundleInfo, string savePath)
		{
			_bundleInfo = bundleInfo;
			_savePath = savePath;
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
				// 检测加载地址是否为空
				if (string.IsNullOrEmpty(_bundleInfo.LocalPath))
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = "Local path is null or empty.";
					return;
				}

				if (string.IsNullOrEmpty(_bundleInfo.RemoteMainURL))
					_steps = ESteps.CheckFile;
				else
					_steps = ESteps.DownloadFromWeb;
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
					// 注意：当文件更新之后，需要删除旧文件			
					if (File.Exists(_savePath))
						File.Delete(_savePath);
					_steps = ESteps.CheckFile;
				}
			}

			// 4. 检测文件
			if (_steps == ESteps.CheckFile)
			{
				// 注意：如果原生文件已经存在，则验证其完整性
				if (File.Exists(_savePath))
				{
					bool result = DownloadSystem.CheckContentIntegrity(_savePath, _bundleInfo.SizeBytes, _bundleInfo.CRC);
					if (result)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
						return;
					}
					else
					{
						File.Delete(_savePath);
					}
				}

				if (_bundleInfo.IsBuildinJarFile())
				{
					_steps = ESteps.DownloadFromApk;
				}
				else
				{
					try
					{
						File.Copy(_bundleInfo.LocalPath, _savePath, true);
					}
					catch (System.Exception e)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = e.ToString();
						return;
					}

					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}

			// 5. 从APK拷贝文件
			if (_steps == ESteps.DownloadFromApk)
			{
				string downloadURL = PathHelper.ConvertToWWWPath(_bundleInfo.LocalPath);
				_fileRequester = new UnityWebFileRequester();
				_fileRequester.SendRequest(downloadURL, _savePath);
				_steps = ESteps.CheckDownloadFromApk;
			}

			// 6. 检测APK拷贝文件结果
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
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}

				_fileRequester.Dispose();
			}
		}

		/// <summary>
		/// 获取原生文件的二进制数据
		/// </summary>
		public byte[] GetFileData()
		{
			if (File.Exists(_savePath) == false)
				return null;
			return File.ReadAllBytes(_savePath);
		}

		/// <summary>
		/// 获取原生文件的文本数据
		/// </summary>
		public string GetFileText()
		{
			if (File.Exists(_savePath) == false)
				return string.Empty;
			return File.ReadAllText(_savePath, System.Text.Encoding.UTF8);
		}
	}
}