using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 更新清单操作
	/// </summary>
	public abstract class UpdateManifestOperation : AsyncOperationBase
	{
		/// <summary>
		/// 是否发现了新的补丁清单
		/// </summary>
		public bool FoundNewManifest { protected set; get; }
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdateManifestOperation : UpdateManifestOperation
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
	internal sealed class OfflinePlayModeUpdateManifestOperation : UpdateManifestOperation
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
	/// </summary>
	internal sealed class HostPlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			None,
			LoadWebHash,
			CheckWebHash,
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
		private readonly int _timeout;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;
		private CacheVerifier _cacheVerifier;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal HostPlayModeUpdateManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
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
			_steps = ESteps.LoadWebHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

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

				// Check error
				if (_downloader1.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader1.GetError();
				}
				else
				{
					string webManifestHash = _downloader1.GetText();
					string cachedManifestHash = GetSandboxPatchManifestFileHash(_packageName, _packageVersion);

					// 如果补丁清单文件的哈希值相同
					if (cachedManifestHash == webManifestHash)
					{
						YooLogger.Log($"Patch manifest file hash is not change : {webManifestHash}");
						LoadSandboxPatchManifest(_packageName, _packageVersion);
						FoundNewManifest = false;
						_steps = ESteps.InitVerifyingCache;
					}
					else
					{
						YooLogger.Log($"Patch manifest hash is change : {cachedManifestHash} -> {webManifestHash}");
						FoundNewManifest = true;
						_steps = ESteps.LoadWebManifest;
					}
				}
				_downloader1.Dispose();
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

				// Check error
				if (_downloader2.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader2.GetError();
				}
				else
				{
					// 解析补丁清单			
					if (ParseAndSaveRemotePatchManifest(_packageName, _packageVersion, _downloader2.GetText()))
					{
						_steps = ESteps.InitVerifyingCache;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"URL : {_downloader2.URL} Error : remote patch manifest content is invalid";
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

		/// <summary>
		/// 解析并保存远端请求的补丁清单
		/// </summary>
		private bool ParseAndSaveRemotePatchManifest(string packageName, string packageVersion, string content)
		{
			try
			{
				var remotePatchManifest = PatchManifest.Deserialize(content);
				_impl.SetLocalPatchManifest(remotePatchManifest);

				YooLogger.Log("Save remote patch manifest file.");
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(packageName, packageVersion);
				string savePath = PathHelper.MakePersistentLoadPath(fileName);
				PatchManifest.Serialize(savePath, remotePatchManifest);
				return true;
			}
			catch (Exception e)
			{
				YooLogger.Error(e.ToString());
				return false;
			}
		}

		/// <summary>
		/// 加载沙盒内的补丁清单
		/// 注意：在加载本地补丁清单之前，已经验证过文件的哈希值
		/// </summary>
		private void LoadSandboxPatchManifest(string packageName, string packageVersion)
		{
			YooLogger.Log("Load sandbox patch manifest file.");
			string fileName = YooAssetSettingsData.GetPatchManifestFileName(packageName, packageVersion);
			string filePath = PathHelper.MakePersistentLoadPath(fileName);
			string jsonData = File.ReadAllText(filePath);
			var sandboxPatchManifest = PatchManifest.Deserialize(jsonData);
			_impl.SetLocalPatchManifest(sandboxPatchManifest);
		}

		/// <summary>
		/// 获取沙盒内补丁清单文件的哈希值
		/// 注意：如果沙盒内补丁清单文件不存在，返回空字符串
		/// </summary>
		private string GetSandboxPatchManifestFileHash(string packageName, string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetPatchManifestFileName(packageName, packageVersion);
			string filePath = PathHelper.MakePersistentLoadPath(fileName);
			if (File.Exists(filePath))
				return HashUtility.FileMD5(filePath);
			else
				return string.Empty;
		}
	}

	/// <summary>
	/// 联机模式的更新清单操作（弱联网）
	/// </summary>
	internal sealed class HostPlayModeWeaklyUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			None,
			LoadSandboxManifestHash,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private ESteps _steps = ESteps.None;
		private CacheVerifier _cacheVerifier;
		private float _verifyTime;

		internal HostPlayModeWeaklyUpdateManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;

#if UNITY_WEBGL
			_cacheVerifier = new CacheVerifierWithoutThread();
#else
			_cacheVerifier = new CacheVerifierWithThread();
#endif
		}
		internal override void Start()
		{
			_steps = ESteps.LoadSandboxManifestHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadSandboxManifestHash)
			{
				LoadSandboxPatchManifest(_packageName, _packageVersion);
				_steps = ESteps.InitVerifyingCache;
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				var verifyInfos = _impl.GetVerifyInfoList(true);
				_cacheVerifier.InitVerifier(verifyInfos);
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.UpdateVerifyingCache;
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _cacheVerifier.GetVerifierProgress();
				if (_cacheVerifier.UpdateVerifier())
				{
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_cacheVerifier.VerifySuccessList.Count}, Fail {_cacheVerifier.VerifyFailList.Count}, Elapsed time {costTime} seconds");

					bool verifySucceed = true;
					foreach (var verifyInfo in _cacheVerifier.VerifyFailList)
					{
						// 注意：跳过内置资源文件
						if (verifyInfo.IsBuildinFile)
							continue;

						verifySucceed = false;
						YooLogger.Warning($"Failed verify file : {verifyInfo.VerifyFilePath}");
					}

					if (verifySucceed)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"The package resource {_packageName}_{_packageVersion} content has verify failed file !";
					}
				}
			}
		}

		/// <summary>
		/// 加载沙盒内的补丁清单
		/// 注意：在加载本地补丁清单之前，未验证过文件的哈希值
		/// </summary>
		private void LoadSandboxPatchManifest(string packageName, string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetPatchManifestFileName(packageName, packageVersion);
			string filePath = PathHelper.MakePersistentLoadPath(fileName);
			if (File.Exists(filePath))
			{
				YooLogger.Log("Load sandbox patch manifest file.");
				string jsonData = File.ReadAllText(filePath);
				var sandboxPatchManifest = PatchManifest.Deserialize(jsonData);
				_impl.SetLocalPatchManifest(sandboxPatchManifest);
			}
		}
	}
}