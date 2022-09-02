using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	internal sealed class AssetBundleWebLoader : AssetBundleLoaderBase
	{
		private enum ESteps
		{
			None = 0,
			Download,
			CheckDownload,
			LoadCacheFile,
			CheckLoadCacheFile,
			LoadWebFile,
			CheckLoadWebFile,		
			TryLoadWebFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private float _tryTimer = 0;
		private string _fileLoadPath;
		private bool _isShowWaitForAsyncError = false;
		private DownloaderBase _downloader;
		private UnityWebRequest _webRequest;
		private AssetBundleCreateRequest _createRequest;

		
		public AssetBundleWebLoader(BundleInfo bundleInfo) : base(bundleInfo)
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
					_fileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.LoadWebFile;
					_fileLoadPath = MainBundleInfo.Bundle.StreamingFilePath;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
				{
					_steps = ESteps.LoadCacheFile;
					_fileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
				}
				else
				{
					throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
				}
			}

			// 1. 从服务器下载
			if (_steps == ESteps.Download)
			{
				int failedTryAgain = int.MaxValue;
				_downloader = DownloadSystem.BeginDownload(MainBundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 2. 检测服务器下载结果
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
					_steps = ESteps.LoadCacheFile;
				}
			}

			// 3. 从本地缓存里加载AssetBundle
			if (_steps == ESteps.LoadCacheFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(_fileLoadPath) == false)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"Not found assetBundle file : {_fileLoadPath}";
					YooLogger.Error(LastError);
					return;
				}
#endif

				// Load assetBundle file
				if (MainBundleInfo.Bundle.IsEncrypted)
				{
					if (AssetSystem.DecryptionServices == null)
						throw new Exception($"{nameof(AssetBundleFileLoader)} need {nameof(IDecryptionServices)} : {MainBundleInfo.Bundle.BundleName}");

					DecryptionFileInfo fileInfo = new DecryptionFileInfo();
					fileInfo.BundleName = MainBundleInfo.Bundle.BundleName;
					fileInfo.FileHash = MainBundleInfo.Bundle.FileHash;
					ulong offset = AssetSystem.DecryptionServices.GetFileOffset(fileInfo);
					_createRequest = AssetBundle.LoadFromFileAsync(_fileLoadPath, 0, offset);
				}
				else
				{
					_createRequest = AssetBundle.LoadFromFileAsync(_fileLoadPath);
				}
				_steps = ESteps.CheckLoadCacheFile;
			}

			// 4. 检测AssetBundle加载结果
			if (_steps == ESteps.CheckLoadCacheFile)
			{
				if (_createRequest.isDone == false)
					return;

				CacheBundle = _createRequest.assetBundle;
				if (CacheBundle == null)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"Failed to load AssetBundle file : {MainBundleInfo.Bundle.BundleName}";
					YooLogger.Error(LastError);

					// 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
					// 在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
					if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
					{
						string cacheLoadPath = MainBundleInfo.Bundle.CachedFilePath;
						if (CacheSystem.VerifyBundle(MainBundleInfo.Bundle, EVerifyLevel.High) != EVerifyResult.Succeed)
						{
							if (File.Exists(cacheLoadPath))
							{
								YooLogger.Error($"Delete the invalid cache file : {cacheLoadPath}");
								File.Delete(cacheLoadPath);
							}
						}
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EStatus.Succeed;
				}
			}

			// 5. 从WEB网站获取AssetBundle文件
			if (_steps == ESteps.LoadWebFile)
			{
				_webRequest = UnityWebRequestAssetBundle.GetAssetBundle(_fileLoadPath, Hash128.Parse(MainBundleInfo.Bundle.FileHash));
				_webRequest.SendWebRequest();
				_steps = ESteps.CheckLoadWebFile;
			}

			// 6. 检测AssetBundle加载结果
			if (_steps == ESteps.CheckLoadWebFile)
			{
				if (_webRequest.isDone == false)
					return;

#if UNITY_2020_1_OR_NEWER
				if (_webRequest.result != UnityWebRequest.Result.Success)
#else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
#endif
				{
					YooLogger.Warning($"Failed to get asset bundle from web : {_fileLoadPath} Error : {_webRequest.error}");
					_steps = ESteps.TryLoadWebFile;
					_tryTimer = 0;
				}
				else
				{
					CacheBundle = DownloadHandlerAssetBundle.GetContent(_webRequest);
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
				}
			}

			// 7. 如果获取失败，重新尝试
			if (_steps == ESteps.TryLoadWebFile)
			{
				_tryTimer += Time.unscaledDeltaTime;
				if (_tryTimer > 1f)
				{
					_webRequest.Dispose();
					_webRequest = null;
					_steps = ESteps.LoadWebFile;
				}
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			if (_isShowWaitForAsyncError == false)
			{
				_isShowWaitForAsyncError = true;
				YooLogger.Error($"WebGL platform not support {nameof(WaitForAsyncComplete)} ! Use the async load method instead of the sync load method !");
			}
		}
	}
}