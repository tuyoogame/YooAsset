using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class AssetBundleFileLoader : AssetBundleLoaderBase
	{
		private enum ESteps
		{
			None = 0,
			Download,
			CheckDownload,
			LoadFile,
			CheckFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private bool _isWaitForAsyncComplete = false;
		private bool _isShowWaitForAsyncError = false;
		private DownloaderBase _downloader;
		private AssetBundleCreateRequest _cacheRequest;


		public AssetBundleFileLoader(BundleInfo bundleInfo) : base(bundleInfo)
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
				// 检测加载地址是否为空
				if (string.IsNullOrEmpty(BundleFileInfo.LocalPath))
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					return;
				}

				if (string.IsNullOrEmpty(BundleFileInfo.RemoteMainURL))
					_steps = ESteps.LoadFile;
				else
					_steps = ESteps.Download;
			}

			// 1. 从服务器下载
			if (_steps == ESteps.Download)
			{
				int failedTryAgain = int.MaxValue;
				_downloader = DownloadSystem.BeginDownload(BundleFileInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 2. 检测服务器下载结果
			if (_steps == ESteps.CheckDownload)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_downloader.ReportError();
					_steps = ESteps.Done;
					Status = EStatus.Failed;
				}
				else
				{
					_steps = ESteps.LoadFile;
				}
			}

			// 3. 加载AssetBundle
			if (_steps == ESteps.LoadFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(BundleFileInfo.LocalPath) == false)
				{
					YooLogger.Warning($"Not found assetBundle file : {BundleFileInfo.LocalPath}");
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					return;
				}
#endif

				// Load assetBundle file
				if (BundleFileInfo.IsEncrypted)
				{
					if (AssetSystem.DecryptionServices == null)
						throw new Exception($"{nameof(AssetBundleFileLoader)} need IDecryptServices : {BundleFileInfo.BundleName}");

					ulong offset = AssetSystem.DecryptionServices.GetFileOffset(BundleFileInfo);
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(BundleFileInfo.LocalPath, 0, offset);
					else
						_cacheRequest = AssetBundle.LoadFromFileAsync(BundleFileInfo.LocalPath, 0, offset);
				}
				else
				{
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(BundleFileInfo.LocalPath);
					else
						_cacheRequest = AssetBundle.LoadFromFileAsync(BundleFileInfo.LocalPath);
				}
				_steps = ESteps.CheckFile;
			}

			// 4. 检测AssetBundle加载结果
			if (_steps == ESteps.CheckFile)
			{
				if (_cacheRequest != null)
				{
					if (_isWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						YooLogger.Warning("Suspend the main thread to load unity bundle.");
						CacheBundle = _cacheRequest.assetBundle;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						CacheBundle = _cacheRequest.assetBundle;
					}
				}

				// Check error			
				if (CacheBundle == null)
				{
					YooLogger.Error($"Failed to load assetBundle file : {BundleFileInfo.BundleName}");
					_steps = ESteps.Done;
					Status = EStatus.Failed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EStatus.Succeed;
				}
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			_isWaitForAsyncComplete = true;

			int frame = 1000;
			while (true)
			{
				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
				{
					if (_isShowWaitForAsyncError == false)
					{
						_isShowWaitForAsyncError = true;
						YooLogger.Error($"WaitForAsyncComplete failed ! BundleName : {BundleFileInfo.BundleName} States : {Status}");
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