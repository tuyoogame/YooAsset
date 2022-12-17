using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*
namespace YooAsset
{
	public abstract class DownloadPackageOperation : AsyncOperationBase
	{
		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间（单位：秒）</param>
		public abstract PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout);
	}

	/// <summary>
	/// 编辑器下模拟运行的更新资源包裹操作
	/// </summary>
	internal sealed class EditorPlayModeDownloadPackageOperation : DownloadPackageOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}

		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		public override PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = new List<BundleInfo>();
			var operation = new PackageDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
	}

	/// <summary>
	/// 离线模式的更新资源包裹操作
	/// </summary>
	internal sealed class OfflinePlayModeDownloadPackageOperation : DownloadPackageOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}

		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		public override PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = new List<BundleInfo>();
			var operation = new PackageDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
	}

	/// <summary>
	/// 联机模式的更新资源包裹操作
	/// </summary>
	internal sealed class HostPlayModeDownloadPackageOperation : DownloadPackageOperation
	{
		private enum ESteps
		{
			None,
			LoadWebManifest,
			CheckLoadWebManifest,
			CheckDeserializeManifest,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly int _timeout;
		private ESteps _steps = ESteps.None;
		private UnityWebDataRequester _downloader;
		private DeserializeManifestOperation _deserializer;
		private PatchManifest _remotePatchManifest;

		internal HostPlayModeDownloadPackageOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadWebManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadWebManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckLoadWebManifest;
			}

			if (_steps == ESteps.CheckLoadWebManifest)
			{
				Progress = _downloader.Progress();
				if (_downloader.IsDone() == false)
					return;

				// Check error
				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetError();
				}
				else
				{
					// 解析补丁清单
					byte[] bytesData = _downloader.GetData();
					_deserializer = new DeserializeManifestOperation(bytesData);
					OperationSystem.StartOperation(_deserializer);
					_steps = ESteps.CheckDeserializeManifest;
				}
				_downloader.Dispose();
			}

			if (_steps == ESteps.CheckDeserializeManifest)
			{
				Progress = _deserializer.Progress;
				if (_deserializer.IsDone)
				{
					if (_deserializer.Status == EOperationStatus.Succeed)
					{
						_remotePatchManifest = _deserializer.Manifest;
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = _deserializer.Error;
					}
				}
			}
		}

		/// <summary>
		/// 创建包裹下载器
		/// </summary>
		public override PackageDownloaderOperation CreatePackageDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			if (Status == EOperationStatus.Succeed)
			{
				YooLogger.Log($"Create package downloader : {_remotePatchManifest.PackageName} {_remotePatchManifest.PackageVersion}");
				List<BundleInfo> downloadList = GetDownloadList();
				var operation = new PackageDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
				return operation;
			}
			else
			{
				YooLogger.Error($"{nameof(DownloadPackageOperation)} status is failed !");
				var operation = new PackageDownloaderOperation(null, downloadingMaxNumber, failedTryAgain, timeout);
				return operation;
			}
		}

		/// <summary>
		/// 获取补丁清单请求地址
		/// </summary>
		private string GetPatchManifestRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return _impl.GetPatchDownloadFallbackURL(fileName);
			else
				return _impl.GetPatchDownloadMainURL(fileName);
		}

		/// <summary>
		/// 获取下载列表
		/// </summary>
		private List<BundleInfo> GetDownloadList()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in _remotePatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (CacheSystem.IsCached(patchBundle))
					continue;

				// 忽略APP资源
				if (_impl.IsBuildinPatchBundle(patchBundle))
					continue;

				downloadList.Add(patchBundle);
			}

			return _impl.ConvertToDownloadList(downloadList);
		}
	}
}
*/