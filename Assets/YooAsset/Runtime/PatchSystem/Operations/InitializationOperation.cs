using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 初始化操作
	/// </summary>
	public abstract class InitializationOperation : AsyncOperationBase
	{
	}

	/// <summary>
	/// 编辑器下模拟运行的初始化操作
	/// </summary>
	internal class EditorModeInitializationOperation : InitializationOperation
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
	/// 离线模式的初始化操作
	/// </summary>
	internal class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			LoadAppManifest,
			CheckAppManifest,
			Done,
		}

		private OfflinePlayModeImpl _impl;
		private ESteps _steps = ESteps.None;
		private UnityWebRequester _downloader;
		private string _downloadURL;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl)
		{
			_impl = impl;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadAppManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadAppManifest)
			{
				string filePath = PathHelper.MakeStreamingLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
				_downloadURL = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebRequester();
				_downloader.SendRequest(_downloadURL);
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
					_downloader.Dispose();
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					throw new System.Exception($"Fatal error : Failed load application patch manifest file : {_downloadURL}");
				}

				// 解析APP里的补丁清单
				_impl.AppPatchManifest = PatchManifest.Deserialize(_downloader.GetText());
				_downloader.Dispose();
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}

	/// <summary>
	/// 网络模式的初始化操作
	/// </summary>
	internal class HostPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			InitCache,
			LoadAppManifest,
			CheckAppManifest,
			LoadSandboxManifest,
			Done,
		}

		private HostPlayModeImpl _impl;
		private ESteps _steps = ESteps.None;
		private UnityWebRequester _downloader;
		private string _downloadURL;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl)
		{
			_impl = impl;
		}
		internal override void Start()
		{
			_steps = ESteps.InitCache;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.InitCache)
			{
				// 每次启动时比对APP版本号是否一致	
				PatchCache cache = PatchCache.LoadCache();
				if (cache.CacheAppVersion != Application.version)
				{
					YooLogger.Warning($"Cache is dirty ! Cache app version is {cache.CacheAppVersion}, Current app version is {Application.version}");

					// 注意：在覆盖安装的时候，会保留APP沙盒目录，可以选择清空缓存目录
					if (_impl.ClearCacheWhenDirty)
					{
						YooLogger.Warning("Clear cache files.");
						SandboxHelper.DeleteSandboxCacheFolder();
					}

					// 删除清单文件
					SandboxHelper.DeleteSandboxPatchManifestFile();
					// 更新缓存文件
					PatchCache.UpdateCache();
				}
				_steps = ESteps.LoadAppManifest;
			}

			if (_steps == ESteps.LoadAppManifest)
			{
				// 加载APP内的补丁清单
				YooLogger.Log($"Load application patch manifest.");
				string filePath = PathHelper.MakeStreamingLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
				_downloadURL = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebRequester();
				_downloader.SendRequest(_downloadURL);
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
					_downloader.Dispose();
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					throw new System.Exception($"Fatal error : Failed load application patch manifest file : {_downloadURL}");
				}

				// 解析补丁清单
				string jsonData = _downloader.GetText();
				_impl.AppPatchManifest = PatchManifest.Deserialize(jsonData);
				_impl.LocalPatchManifest = _impl.AppPatchManifest;
				_downloader.Dispose();
				_steps = ESteps.LoadSandboxManifest;
			}

			if (_steps == ESteps.LoadSandboxManifest)
			{
				// 加载沙盒内的补丁清单	
				if (SandboxHelper.CheckSandboxPatchManifestFileExist())
				{
					YooLogger.Log($"Load sandbox patch manifest.");
					string filePath = PathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
					string jsonData = File.ReadAllText(filePath);
					_impl.LocalPatchManifest = PatchManifest.Deserialize(jsonData);
				}

				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}
}