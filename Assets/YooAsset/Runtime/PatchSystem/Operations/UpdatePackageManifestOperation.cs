using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 向远端请求并更新补丁清单
	/// </summary>
	public abstract class UpdatePackageManifestOperation : AsyncOperationBase
	{
		internal IPlayModeServices _playModeServices;
		internal PatchManifest _patchManifest;

		/// <summary>
		/// 是否发现了新的补丁清单
		/// </summary>
		public bool FoundNewManifest { protected set; get; } = false;

		#region 资源下载
		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call downloader !");
				return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}
			return _playModeServices.CreatePatchDownloaderByTags(_patchManifest, new string[] { tag }, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call downloader !");
				return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}
			return _playModeServices.CreatePatchDownloaderByTags(_patchManifest, tags, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call downloader !");
				return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}
			return _playModeServices.CreatePatchDownloaderByAll(_patchManifest, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call downloader !");
				return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			}
			return _playModeServices.CreatePatchDownloaderByPaths(_patchManifest, assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}
		#endregion

		#region 资源解压
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call unpacker !");
				return PatchUnpackerOperation.CreateEmptyUnpacker(unpackingMaxNumber, failedTryAgain, int.MaxValue);
			}
			return _playModeServices.CreatePatchUnpackerByTags(_patchManifest, new string[] { tag }, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call unpacker !");
				return PatchUnpackerOperation.CreateEmptyUnpacker(unpackingMaxNumber, failedTryAgain, int.MaxValue);
			}
			return _playModeServices.CreatePatchUnpackerByTags(_patchManifest, tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public PatchUnpackerOperation CreatePatchUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			if (Status != EOperationStatus.Succeed)
			{
				YooLogger.Error($"Please check { nameof(UpdatePackageManifestOperation)} status before call unpacker !");
				return PatchUnpackerOperation.CreateEmptyUnpacker(unpackingMaxNumber, failedTryAgain, int.MaxValue);
			}
			return _playModeServices.CreatePatchUnpackerByAll(_patchManifest, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}
		#endregion
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public EditorPlayModeUpdatePackageManifestOperation(EditorSimulateModeImpl impl)
		{
			_playModeServices = impl;
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的更新清单操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public OfflinePlayModeUpdatePackageManifestOperation(OfflinePlayModeImpl impl)
		{
			_playModeServices = impl;
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 联机模式的更新清单操作
	/// 注意：优先比对沙盒清单哈希值，如果有变化就更新远端清单文件，并保存到本地。
	/// </summary>
	internal sealed class HostPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		private enum ESteps
		{
			None,
			TryLoadCacheHash,
			DownloadWebHash,
			CheckDownloadWebHash,
			DownloadWebManifest,
			CheckDownloadWebManifest,
			CheckDeserializeWebManifest,
			StartVerifyOperation,
			CheckVerifyOperation,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private bool _autoSaveManifest;
		private bool _autoActiveManifest;
		private readonly int _timeout;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;
		private DeserializeManifestOperation _deserializer;
		private CacheFilesVerifyOperation _verifyOperation;

		private string _cacheManifestHash;
		private ESteps _steps = ESteps.None;
		private byte[] _fileBytes = null;
		private float _verifyTime;

		internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, bool autoSaveManifest, bool autoActiveManifest, int timeout)
		{
			_playModeServices = impl;
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_autoSaveManifest = autoSaveManifest;
			_autoActiveManifest = autoActiveManifest;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.TryLoadCacheHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.TryLoadCacheHash)
			{
				string filePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
				if (File.Exists(filePath))
				{				
					_cacheManifestHash = HashUtility.FileMD5(filePath);
					_steps = ESteps.DownloadWebHash;
				}
				else
				{
					_steps = ESteps.DownloadWebManifest;
				}
			}

			if (_steps == ESteps.DownloadWebHash)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestHashFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckDownloadWebHash;
			}

			if (_steps == ESteps.CheckDownloadWebHash)
			{
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader1.GetError();
				}
				else
				{
					string webManifestHash = _downloader1.GetText();
					if (_cacheManifestHash == webManifestHash)
					{
						YooLogger.Log($"Not found new package : {_packageName}");
						_patchManifest = _impl.ActivePatchManifest;
						FoundNewManifest = false;
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
					else
					{
						YooLogger.Log($"Package {_packageName} is change : {_cacheManifestHash} -> {webManifestHash}");
						FoundNewManifest = true;
						_steps = ESteps.DownloadWebManifest;
					}
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.DownloadWebManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloader2 = new UnityWebDataRequester();
				_downloader2.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckDownloadWebManifest;
			}

			if (_steps == ESteps.CheckDownloadWebManifest)
			{
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader2.GetError();
				}
				else
				{
					byte[] bytesData = _downloader2.GetData();

					// 保存文件到沙盒内
					if (_autoSaveManifest)
					{
						string savePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
						FileUtility.CreateFile(savePath, bytesData);
					}
					else
					{
						_fileBytes = bytesData;
					}

					// 解析二进制数据
					_deserializer = new DeserializeManifestOperation(bytesData);
					OperationSystem.StartOperation(_deserializer);
					_steps = ESteps.CheckDeserializeWebManifest;
				}

				_downloader2.Dispose();
			}

			if (_steps == ESteps.CheckDeserializeWebManifest)
			{
				Progress = _deserializer.Progress;
				if (_deserializer.IsDone)
				{
					if (_deserializer.Status == EOperationStatus.Succeed)
					{
						if (_autoActiveManifest)
						{
							_impl.ActivePatchManifest = _deserializer.Manifest;
						}

						_patchManifest = _deserializer.Manifest;
						_steps = ESteps.StartVerifyOperation;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = _deserializer.Error;				
					}
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
#if UNITY_WEBGL
				_verifyOperation = new CacheFilesVerifyWithoutThreadOperation(_deserializer.Manifest, _impl);
#else
				_verifyOperation = new CacheFilesVerifyWithThreadOperation(_deserializer.Manifest, _impl);
#endif

				OperationSystem.StartOperation(_verifyOperation);
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.CheckVerifyOperation;
			}

			if (_steps == ESteps.CheckVerifyOperation)
			{
				Progress = _verifyOperation.Progress;
				if (_verifyOperation.IsDone)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_verifyOperation.VerifySuccessList.Count}, Fail {_verifyOperation.VerifyFailList.Count}, Elapsed time {costTime} seconds");
				}
			}
		}

		/// <summary>
		/// 手动保存清单文件
		/// </summary>
		public void SaveManifest()
		{
			if (_autoSaveManifest == false)
			{
				if (_fileBytes != null)
				{
					_autoSaveManifest = true;
					string savePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
					FileUtility.CreateFile(savePath, _fileBytes);
				}
			}
		}

		/// <summary>
		/// 手动激活清单文件
		/// </summary>
		public void ActiveManifest()
		{
			if (_autoActiveManifest == false)
			{
				if (_deserializer.Status == EOperationStatus.Succeed)
				{
					_autoActiveManifest = true;
					_impl.ActivePatchManifest = _deserializer.Manifest;
				}
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
	}
}