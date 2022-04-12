using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
	/// <summary>
	/// 更新清单操作
	/// </summary>
	public abstract class UpdateManifestOperation : AsyncOperationBase
	{
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal class EditorModeUpdateManifestOperation : UpdateManifestOperation
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
	internal class OfflinePlayModeUpdateManifestOperation : UpdateManifestOperation
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
	/// 网络模式的更新清单操作
	/// </summary>
	internal class HostPlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			None,
			LoadWebManifestHash,
			CheckWebManifestHash,
			LoadWebManifest,
			CheckWebManifest,
			InitPrepareCache,
			UpdatePrepareCache,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly int _updateResourceVersion;
		private readonly int _timeout;
		private ESteps _steps = ESteps.None;
		private UnityWebDataRequester _downloaderHash;
		private UnityWebDataRequester _downloaderManifest;
		private float _verifyTime;

		public HostPlayModeUpdateManifestOperation(HostPlayModeImpl impl, int updateResourceVersion, int timeout)
		{
			_impl = impl;
			_updateResourceVersion = updateResourceVersion;
			_timeout = timeout;
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
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestHashFileName(_updateResourceVersion));
				YooLogger.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloaderHash = new UnityWebDataRequester();
				_downloaderHash.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifestHash;
			}

			if (_steps == ESteps.CheckWebManifestHash)
			{
				if (_downloaderHash.IsDone() == false)
					return;

				// Check error
				if (_downloaderHash.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloaderHash.GetError();
				}
				else
				{
					string webManifestHash = _downloaderHash.GetText();
					string cachedManifestHash = GetSandboxPatchManifestFileHash(_updateResourceVersion);

					// 如果补丁清单文件的哈希值相同
					if (cachedManifestHash == webManifestHash)
					{
						YooLogger.Log($"Patch manifest file hash is not change : {webManifestHash}");
						_steps = ESteps.InitPrepareCache;
					}
					else
					{
						YooLogger.Log($"Patch manifest hash is change : {webManifestHash} -> {cachedManifestHash}");
						_steps = ESteps.LoadWebManifest;
					}
				}
				_downloaderHash.Dispose();
			}

			if (_steps == ESteps.LoadWebManifest)
			{
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestFileName(_updateResourceVersion));
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloaderManifest = new UnityWebDataRequester();
				_downloaderManifest.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifest;
			}

			if (_steps == ESteps.CheckWebManifest)
			{
				if (_downloaderManifest.IsDone() == false)
					return;

				// Check error
				if (_downloaderManifest.HasError())
				{			
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloaderManifest.GetError();
				}
				else
				{
					// 解析补丁清单			
					if (ParseAndSaveRemotePatchManifest(_updateResourceVersion, _downloaderManifest.GetText()))
					{
						_steps = ESteps.InitPrepareCache;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"URL : {_downloaderManifest.URL} Error : remote patch manifest content is invalid";
					}
				}
				_downloaderManifest.Dispose();
			}

			if (_steps == ESteps.InitPrepareCache)
			{
				InitPrepareCache();
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.UpdatePrepareCache;
			}

			if (_steps == ESteps.UpdatePrepareCache)
			{
				if (UpdatePrepareCache())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify files total time : {costTime}");
				}
			}
		}

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
				_impl.LocalPatchManifest = PatchManifest.Deserialize(content);

				YooLogger.Log("Save remote patch manifest file.");
				string savePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
				PatchManifest.Serialize(savePath, _impl.LocalPatchManifest);
				return true;
			}
			catch (Exception e)
			{
				YooLogger.Warning(e.ToString());
				return false;
			}
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

		#region 多线程相关
		private class ThreadInfo
		{
			public bool Result = false;
			public string FilePath { private set; get; }
			public PatchBundle Bundle { private set; get; }
			public ThreadInfo(string filePath, PatchBundle bundle)
			{
				FilePath = filePath;
				Bundle = bundle;
			}
		}

		private readonly List<PatchBundle> _cacheList = new List<PatchBundle>(1000);
		private readonly List<PatchBundle> _verifyList = new List<PatchBundle>(100);
		private readonly ThreadSyncContext _syncContext = new ThreadSyncContext();
		private const int VerifyMaxCount = 32;

		private void InitPrepareCache()
		{
			// 遍历所有文件然后验证并缓存合法文件
			foreach (var patchBundle in _impl.LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (_impl.AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				// 查看文件是否存在
				string filePath = SandboxHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
				if (File.Exists(filePath) == false)
					continue;

				_cacheList.Add(patchBundle);
			}
		}
		private bool UpdatePrepareCache()
		{
			_syncContext.Update();

			if (_cacheList.Count == 0 && _verifyList.Count == 0)
				return true;

			if (_verifyList.Count >= VerifyMaxCount)
				return false;

			for (int i = _cacheList.Count - 1; i >= 0; i--)
			{
				if (_verifyList.Count >= VerifyMaxCount)
					break;

				var patchBundle = _cacheList[i];
				if (RunThread(patchBundle))
				{
					_cacheList.RemoveAt(i);
					_verifyList.Add(patchBundle);
				}
				else
				{
					YooLogger.Warning("Failed to run verify thread.");
					break;
				}
			}

			return false;
		}
		private bool RunThread(PatchBundle patchBundle)
		{
			string filePath = SandboxHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
			ThreadInfo info = new ThreadInfo(filePath, patchBundle);
			return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyFile), info);
		}
		private void VerifyFile(object infoObj)
		{
			// 验证沙盒内的文件
			ThreadInfo info = (ThreadInfo)infoObj;
			info.Result = DownloadSystem.CheckContentIntegrity(info.FilePath, info.Bundle.SizeBytes, info.Bundle.CRC);
			_syncContext.Post(VerifyCallback, info);
		}
		private void VerifyCallback(object obj)
		{
			ThreadInfo info = (ThreadInfo)obj;
			if (info.Result)
				DownloadSystem.CacheVerifyFile(info.Bundle.Hash, info.Bundle.BundleName);
			_verifyList.Remove(info.Bundle);
		}
		#endregion
	}
}