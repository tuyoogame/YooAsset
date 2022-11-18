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
		/// <summary>
		/// 是否发现了新的补丁清单
		/// </summary>
		public bool FoundNewManifest { protected set; get; } = false;
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
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
			LoadWebHash,
			CheckWebHash,
			LoadCacheManifest,
			LoadWebManifest,
			CheckWebManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly CacheVerifier _cacheVerifier;
		private readonly int _timeout;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;

		private string _cacheManifestHash;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_timeout = timeout;

#if UNITY_WEBGL
			_cacheVerifier = new CacheVerifierWithoutThread();
#else
			_cacheVerifier = new CacheVerifierWithThread();
#endif
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
					_steps = ESteps.LoadWebHash;
				}
				else
				{
					_steps = ESteps.LoadWebManifest;
				}
			}

			if (_steps == ESteps.LoadWebHash)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestHashFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebHash;
			}

			if (_steps == ESteps.CheckWebHash)
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
						_steps = ESteps.LoadCacheManifest;
					}
					else
					{
						YooLogger.Log($"Package {_packageName} is change : {_cacheManifestHash} -> {webManifestHash}");
						_steps = ESteps.LoadWebManifest;
					}
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.LoadCacheManifest)
			{
				try
				{
					var manifest = PersistentHelper.LoadCacheManifestFile(_packageName);
					_impl.SetLocalPatchManifest(manifest);
					_steps = ESteps.InitVerifyingCache;
				}
				catch (System.Exception e)
				{
					// 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，我们主动把损坏的文件删除。
					YooLogger.Warning($"Failed to load cache manifest file : {e.Message}");
					PersistentHelper.DeleteCacheManifestFile(_packageName);
					_steps = ESteps.LoadWebManifest;
				}
			}

			if (_steps == ESteps.LoadWebManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloader2 = new UnityWebDataRequester();
				_downloader2.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifest;
			}

			if (_steps == ESteps.CheckWebManifest)
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
					try
					{
						string content = _downloader2.GetText();
						var manifest = PersistentHelper.SaveCacheManifestFile(_packageName, content);
						_impl.SetLocalPatchManifest(manifest);
						FoundNewManifest = true;
						_steps = ESteps.InitVerifyingCache;
					}
					catch (Exception e)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = e.Message;
					}
				}
				_downloader2.Dispose();
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				var verifyInfos = _impl.GetVerifyInfoList(false);
				_cacheVerifier.InitVerifier(verifyInfos);
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.UpdateVerifyingCache;
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _cacheVerifier.GetVerifierProgress();
				if (_cacheVerifier.UpdateVerifier())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_cacheVerifier.VerifySuccessList.Count}, Fail {_cacheVerifier.VerifyFailList.Count}, Elapsed time {costTime} seconds");
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