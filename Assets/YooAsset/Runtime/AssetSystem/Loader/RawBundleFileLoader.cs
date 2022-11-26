using System.IO;

namespace YooAsset
{
	internal class RawBundleFileLoader : BundleLoaderBase
	{
		private enum ESteps
		{
			None,
			Download,
			CheckDownload,
			Unpack,
			CheckUnpack,
			CheckFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private bool _isShowWaitForAsyncError = false;
		private DownloaderBase _unpacker;
		private DownloaderBase _downloader;


		public RawBundleFileLoader(AssetSystemImpl impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
		{
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public override void Update()
		{
			if (_steps == ESteps.Done)
				return;

			if (_steps == ESteps.None)
			{
				if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				{
					_steps = ESteps.Download;
					FileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
#if UNITY_ANDROID || UNITY_WEBGL
					_steps = ESteps.Unpack;
					FileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
#else
					_steps = ESteps.CheckFile;
					FileLoadPath = MainBundleInfo.Bundle.StreamingFilePath;
#endif
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
				{
					_steps = ESteps.CheckFile;
					FileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
				}
				else
				{
					throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
				}
			}

			// 1. 下载远端文件
			if (_steps == ESteps.Download)
			{
				int failedTryAgain = int.MaxValue;
				_downloader = DownloadSystem.BeginDownload(MainBundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 2. 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = _downloader.GetLastError();
				}
				else
				{
					_steps = ESteps.CheckFile;
				}
			}

			// 3. 解压内置文件
			if (_steps == ESteps.Unpack)
			{
				int failedTryAgain = 1;
				var bundleInfo = HostPlayModeImpl.ConvertToUnpackInfo(MainBundleInfo.Bundle);
				_unpacker = DownloadSystem.BeginDownload(bundleInfo, failedTryAgain);
				_steps = ESteps.CheckUnpack;
			}

			// 4. 检测解压结果
			if (_steps == ESteps.CheckUnpack)
			{
				if (_unpacker.IsDone() == false)
					return;

				if (_unpacker.HasError())
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = _unpacker.GetLastError();
				}
				else
				{
					_steps = ESteps.CheckFile;
				}
			}

			// 5. 检测结果
			if (_steps == ESteps.CheckFile)
			{
				_steps = ESteps.Done;
				if (File.Exists(FileLoadPath))
				{
					Status = EStatus.Succeed;
				}
				else
				{
					Status = EStatus.Failed;
					LastError = $"Raw file not found : {FileLoadPath}";
				}
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			int frame = 1000;
			while (true)
			{
				// 文件解压
				if (_unpacker != null)
				{
					_unpacker.Update();
					if (_unpacker.IsDone() == false)
						continue;
				}

				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
				{
					if (_isShowWaitForAsyncError == false)
					{
						_isShowWaitForAsyncError = true;
						YooLogger.Error($"WaitForAsyncComplete failed ! Try load bundle : {MainBundleInfo.Bundle.BundleName} from remote with sync load method !");
					}
					break;
				}

				// 驱动流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
		}
	}
}