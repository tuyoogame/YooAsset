using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 原生文件操作
	/// </summary>
	public abstract class RawFileOperation : AsyncOperationBase
	{
		internal readonly BundleInfo _bundleInfo;

		/// <summary>
		/// 原生文件的拷贝路径
		/// </summary>
		public string CopyPath { private set; get; }


		internal RawFileOperation(BundleInfo bundleInfo, string copyPath)
		{
			_bundleInfo = bundleInfo;
			CopyPath = copyPath;
		}

		/// <summary>
		/// 原生文件的缓存路径
		/// </summary>
		public abstract string GetCachePath();

		/// <summary>
		/// 获取原生文件的二进制数据
		/// </summary>
		public byte[] LoadFileData()
		{
			string filePath = GetCachePath();
			if (File.Exists(filePath) == false)
				return null;
			return File.ReadAllBytes(filePath);
		}

		/// <summary>
		/// 获取原生文件的文本数据
		/// </summary>
		public string LoadFileText()
		{
			string filePath = GetCachePath();
			if (File.Exists(filePath) == false)
				return string.Empty;
			return File.ReadAllText(filePath, System.Text.Encoding.UTF8);
		}
	}

	/// <summary>
	/// 发生错误的原生文件操作
	/// </summary>
	internal sealed class CompletedRawFileOperation : RawFileOperation
	{
		private readonly string _error;
		internal CompletedRawFileOperation(string error, string copyPath) : base(null, copyPath)
		{
			_error = error;
		}
		internal override void Start()
		{
			Status = EOperationStatus.Failed;
			Error = _error;
		}
		internal override void Update()
		{
		}

		/// <summary>
		/// 原生文件的缓存路径
		/// </summary>
		public override string GetCachePath()
		{
			return string.Empty;
		}
	}

	/// <summary>
	/// 编辑器下模拟运行的原生文件操作
	/// </summary>
	internal sealed class EditorPlayModeRawFileOperation : RawFileOperation
	{
		private enum ESteps
		{
			None,
			Prepare,
			CheckAndCopyFile,
			Done,
		}

		private ESteps _steps = ESteps.None;

		internal EditorPlayModeRawFileOperation(BundleInfo bundleInfo, string copyPath) : base(bundleInfo, copyPath)
		{
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
				if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromEditor)
				{
					_steps = ESteps.CheckAndCopyFile;
					return; // 模拟实现异步操作
				}
				else
				{
					throw new System.NotImplementedException(_bundleInfo.LoadMode.ToString());
				}
			}

			// 2. 检测并拷贝原生文件
			if (_steps == ESteps.CheckAndCopyFile)
			{
				// 如果不需要保存文件
				if (string.IsNullOrEmpty(CopyPath))
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					return;
				}

				// 如果原生文件已经存在，则将其删除
				if (File.Exists(CopyPath))
				{
					File.Delete(CopyPath);
				}

				try
				{
					FileUtility.CreateFileDirectory(CopyPath);
					File.Copy(GetCachePath(), CopyPath, true);
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
		/// 原生文件的缓存路径
		/// </summary>
		public override string GetCachePath()
		{
			if (_bundleInfo == null)
				return string.Empty;
			return _bundleInfo.EditorAssetPath;
		}
	}

	/// <summary>
	/// 离线模式的原生文件操作
	/// </summary>
	internal sealed class OfflinePlayModeRawFileOperation : RawFileOperation
	{
		private enum ESteps
		{
			None,
			Prepare,
			DownloadBuildinFile,
			CheckDownload,
			CheckAndCopyFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private DownloaderBase _downloader;

		public OfflinePlayModeRawFileOperation(BundleInfo bundleInfo, string copyPath) : base(bundleInfo, copyPath)
		{
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
					Error = $"Bundle info is invalid : {_bundleInfo.Bundle.BundleName}";
				}
				else if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.DownloadBuildinFile;
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

			// 2. 下载文件
			if (_steps == ESteps.DownloadBuildinFile)
			{
				int failedTryAgain = int.MaxValue;
				var bundleInfo = PatchHelper.ConvertToUnpackInfo(_bundleInfo.Bundle);
				_downloader = DownloadSystem.BeginDownload(bundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 3. 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				Progress = _downloader.DownloadProgress;
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

			// 4. 检测并拷贝原生文件
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
					var verifyResult = CacheSystem.VerifyContentInternal(CopyPath, _bundleInfo.Bundle.FileSize, _bundleInfo.Bundle.FileCRC, EVerifyLevel.High);
					if (verifyResult == EVerifyResult.Succeed)
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
					File.Copy(GetCachePath(), CopyPath, true);
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
		/// 原生文件的缓存路径
		/// </summary>
		public override string GetCachePath()
		{
			if (_bundleInfo == null)
				return string.Empty;
			return _bundleInfo.Bundle.CachedFilePath;
		}
	}

	/// <summary>
	/// 联机模式的原生文件操作
	/// </summary>
	internal sealed class HostPlayModeRawFileOperation : RawFileOperation
	{
		private enum ESteps
		{
			None,
			Prepare,
			DownloadWebFile,
			DownloadBuildinFile,
			CheckDownload,
			CheckAndCopyFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private DownloaderBase _downloader;

		internal HostPlayModeRawFileOperation(BundleInfo bundleInfo, string copyPath) : base(bundleInfo, copyPath)
		{
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
					Error = $"Bundle info is invalid : {_bundleInfo.Bundle.BundleName}";
				}
				else if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				{
					_steps = ESteps.DownloadWebFile;
				}
				else if (_bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.DownloadBuildinFile;
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

			// 2. 下载远端文件
			if (_steps == ESteps.DownloadWebFile)
			{
				int failedTryAgain = int.MaxValue;
				_downloader = DownloadSystem.BeginDownload(_bundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 3. 下载内置文件
			if (_steps == ESteps.DownloadBuildinFile)
			{
				int failedTryAgain = int.MaxValue;
				var bundleInfo = PatchHelper.ConvertToUnpackInfo(_bundleInfo.Bundle);
				_downloader = DownloadSystem.BeginDownload(bundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 4. 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				Progress = _downloader.DownloadProgress;
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

			// 5. 检测并拷贝原生文件
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
					var verifyResult = CacheSystem.VerifyContentInternal(CopyPath, _bundleInfo.Bundle.FileSize, _bundleInfo.Bundle.FileCRC, EVerifyLevel.High);
					if (verifyResult == EVerifyResult.Succeed)
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
					File.Copy(GetCachePath(), CopyPath, true);
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
		/// 原生文件的缓存路径
		/// </summary>
		public override string GetCachePath()
		{
			if (_bundleInfo == null)
				return string.Empty;
			return _bundleInfo.Bundle.CachedFilePath;
		}
	}
}