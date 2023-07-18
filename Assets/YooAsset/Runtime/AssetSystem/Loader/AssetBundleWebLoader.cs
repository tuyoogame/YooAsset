using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	/// <summary>
	/// WebGL平台加载器
	/// </summary>
	internal sealed class AssetBundleWebLoader : BundleLoaderBase
	{
		private enum ESteps
		{
			None = 0,
			LoadWebSiteFile,
			LoadRemoteFile,
			CheckLoadFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private WebDownloader _downloader;


		public AssetBundleWebLoader(AssetSystemImpl impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
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
					_steps = ESteps.LoadRemoteFile;
					FileLoadPath = string.Empty;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.LoadWebSiteFile;
					FileLoadPath = string.Empty;
				}
				else
				{
					throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
				}
			}

			// 1. 跨域获取资源包
			if (_steps == ESteps.LoadRemoteFile)
			{
				int failedTryAgain = Impl.DownloadFailedTryAgain;
				_downloader = DownloadSystem.CreateDownload(MainBundleInfo, failedTryAgain) as WebDownloader;
				_downloader.SendRequest(true);
				_steps = ESteps.CheckLoadFile;
			}

			// 2. 从站点获取资源包
			if (_steps == ESteps.LoadWebSiteFile)
			{
				int failedTryAgain = Impl.DownloadFailedTryAgain;
				var bundleInfo = ManifestTools.ConvertToUnpackInfo(MainBundleInfo.Bundle);
				_downloader = DownloadSystem.CreateDownload(bundleInfo, failedTryAgain) as WebDownloader;
				_downloader.SendRequest(true);
				_steps = ESteps.CheckLoadFile;
			}

			// 3. 检测加载结果
			if (_steps == ESteps.CheckLoadFile)
			{
				DownloadProgress = _downloader.DownloadProgress;
				DownloadedBytes = _downloader.DownloadedBytes;
				if (_downloader.IsDone() == false)
					return;

				CacheBundle = _downloader.GetAssetBundle();
				if (CacheBundle == null)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"AssetBundle file is invalid : {MainBundleInfo.Bundle.BundleName}";
					YooLogger.Error(LastError);
				}
				else
				{
					_steps = ESteps.Done;
					Status = EStatus.Succeed;
				}

				// 注意：释放下载句柄
				_downloader.DisposeHandler();
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			if (IsDone() == false)
			{
				Status = EStatus.Failed;
				LastError = $"{nameof(WaitForAsyncComplete)} failed ! WebGL platform not support sync load method !";
				YooLogger.Error(LastError);
			}
		}
	}
}