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
			LoadWebManifestHash,
			CheckWebManifestHash,
			LoadWebManifest,
			CheckWebManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly int _resourceVersion;
		private readonly int _timeout;
		private ESteps _steps = ESteps.None;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;
		private PatchCacheVerifier _patchCacheVerifier;
		private float _verifyTime;

		internal HostPlayModeUpdateManifestOperation(HostPlayModeImpl impl, int resourceVersion, int timeout)
		{
			_impl = impl;
			_resourceVersion = resourceVersion;
			_timeout = timeout;

#if UNITY_WEBGL
			_patchCacheVerifier = new PatchCacheVerifierWithoutThread();
#else
			_patchCacheVerifier = new PatchCacheVerifierWithThread();
#endif
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadWebManifestHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadWebManifestHash)
			{
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestHashFileName(_resourceVersion));
				YooLogger.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifestHash;
			}

			if (_steps == ESteps.CheckWebManifestHash)
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
					string cachedManifestHash = GetSandboxPatchManifestFileHash(_resourceVersion);

					// 如果补丁清单文件的哈希值相同
					if (cachedManifestHash == webManifestHash)
					{
						YooLogger.Log($"Patch manifest file hash is not change : {webManifestHash}");
						LoadSandboxPatchManifest(_resourceVersion);
						FoundNewManifest = false;
						_steps = ESteps.InitVerifyingCache;
					}
					else
					{
						YooLogger.Log($"Patch manifest hash is change : {webManifestHash} -> {cachedManifestHash}");
						FoundNewManifest = true;
						_steps = ESteps.LoadWebManifest;
					}
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.LoadWebManifest)
			{
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestFileName(_resourceVersion));
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
					if (ParseAndSaveRemotePatchManifest(_resourceVersion, _downloader2.GetText()))
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
				_patchCacheVerifier.InitVerifier(_impl.AppPatchManifest, _impl.LocalPatchManifest, false);
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.UpdateVerifyingCache;
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _patchCacheVerifier.GetVerifierProgress();
				if (_patchCacheVerifier.UpdateVerifier())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_patchCacheVerifier.VerifySuccessCount}, Fail {_patchCacheVerifier.VerifyFailCount}, Elapsed time {costTime} seconds");
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
		private bool ParseAndSaveRemotePatchManifest(int updateResourceVersion, string content)
		{
			try
			{
				var remotePatchManifest = PatchManifest.Deserialize(content);
				_impl.SetLocalPatchManifest(remotePatchManifest);

				YooLogger.Log("Save remote patch manifest file.");
				string savePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
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
		private void LoadSandboxPatchManifest(int updateResourceVersion)
		{
			YooLogger.Log("Load sandbox patch manifest file.");
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
			string jsonData = File.ReadAllText(filePath);
			var sandboxPatchManifest = PatchManifest.Deserialize(jsonData);
			_impl.SetLocalPatchManifest(sandboxPatchManifest);
		}

		/// <summary>
		/// 获取沙盒内补丁清单文件的哈希值
		/// 注意：如果沙盒内补丁清单文件不存在，返回空字符串
		/// </summary>
		private string GetSandboxPatchManifestFileHash(int updateResourceVersion)
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
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
		private readonly int _resourceVersion;
		private ESteps _steps = ESteps.None;
		private PatchCacheVerifier _patchCacheVerifier;
		private float _verifyTime;

		internal HostPlayModeWeaklyUpdateManifestOperation(HostPlayModeImpl impl, int resourceVersion)
		{
			_impl = impl;
			_resourceVersion = resourceVersion;

#if UNITY_WEBGL
			_patchCacheVerifier = new PatchCacheVerifierWithoutThread();
#else
			_patchCacheVerifier = new PatchCacheVerifierWithThread();
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
				LoadSandboxPatchManifest(_resourceVersion);
				_steps = ESteps.InitVerifyingCache;
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				if (_patchCacheVerifier.InitVerifier(_impl.AppPatchManifest, _impl.LocalPatchManifest, true))
				{
					_verifyTime = UnityEngine.Time.realtimeSinceStartup;
					_steps = ESteps.UpdateVerifyingCache;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"The resource version {_resourceVersion} content is not complete !";
				}
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _patchCacheVerifier.GetVerifierProgress();
				if (_patchCacheVerifier.UpdateVerifier())
				{
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_patchCacheVerifier.VerifySuccessCount}, Fail {_patchCacheVerifier.VerifyFailCount}, Elapsed time {costTime} seconds");
					if (_patchCacheVerifier.VerifyFailCount > 0)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"The resource version {_resourceVersion} content has verify failed file !";
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
			}
		}

		/// <summary>
		/// 加载沙盒内的补丁清单
		/// 注意：在加载本地补丁清单之前，未验证过文件的哈希值
		/// </summary>
		private void LoadSandboxPatchManifest(int updateResourceVersion)
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
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