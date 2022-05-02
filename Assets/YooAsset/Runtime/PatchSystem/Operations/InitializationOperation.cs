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
	internal sealed class EditorPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			Builder,
			Done,
		}

		private readonly EditorPlayModeImpl _impl;
		private ESteps _steps = ESteps.None;

		internal EditorPlayModeInitializationOperation(EditorPlayModeImpl impl)
		{
			_impl = impl;
		}
		internal override void Start()
		{
			_steps = ESteps.Builder;
		}
		internal override void Update()
		{
			if (_steps == ESteps.Builder)
			{
				string manifestFilePath = EditorPlayModeHelper.DryRunBuild();
				if (string.IsNullOrEmpty(manifestFilePath))
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = "Dry run build failed, see the detail info on the console window.";
					return;
				}
				if (File.Exists(manifestFilePath) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Manifest file not found : {manifestFilePath}";
					return;
				}

				YooLogger.Log($"Load manifest file in editor play mode : {manifestFilePath}");
				string jsonContent = FileUtility.ReadFile(manifestFilePath);
				_impl.AppPatchManifest = PatchManifest.Deserialize(jsonContent);
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}

	/// <summary>
	/// 离线模式的初始化操作
	/// </summary>
	internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			Update,
			Done,
		}

		private readonly OfflinePlayModeImpl _impl;
		private readonly AppManifestLoader _appManifestLoader = new AppManifestLoader();
		private ESteps _steps = ESteps.None;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl)
		{
			_impl = impl;
		}
		internal override void Start()
		{
			_steps = ESteps.Update;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.Update)
			{
				_appManifestLoader.Update();
				if (_appManifestLoader.IsDone() == false)
					return;

				if (_appManifestLoader.Result == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
					throw new System.Exception($"FATAL : {_appManifestLoader.Error}");
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					_impl.AppPatchManifest = _appManifestLoader.Result;
				}
			}
		}
	}

	/// <summary>
	/// 网络模式的初始化操作
	/// </summary>
	internal sealed class HostPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			InitCache,
			Update,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly AppManifestLoader _appManifestLoader = new AppManifestLoader();
		private ESteps _steps = ESteps.None;

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
				CacheData cacheData = CacheData.LoadCache();
				if (cacheData.CacheAppVersion != Application.version)
				{
					YooLogger.Warning($"Cache is dirty ! Cache application version is {cacheData.CacheAppVersion}, Current application version is {Application.version}");

					// 注意：在覆盖安装的时候，会保留APP沙盒目录，可以选择清空缓存目录
					if (_impl.ClearCacheWhenDirty)
					{
						YooLogger.Warning("Clear cache files.");
						SandboxHelper.DeleteCacheFolder();
					}

					// 更新缓存文件
					CacheData.UpdateCache();
				}
				_steps = ESteps.Update;
			}

			if (_steps == ESteps.Update)
			{
				_appManifestLoader.Update();
				if (_appManifestLoader.IsDone() == false)
					return;

				if (_appManifestLoader.Result == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
					throw new System.Exception($"FATAL : {_appManifestLoader.Error}");
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					_impl.AppPatchManifest = _appManifestLoader.Result;
					_impl.LocalPatchManifest = _appManifestLoader.Result;
				}
			}
		}
	}


	/// <summary>
	/// 内置补丁清单加载器
	/// </summary>
	internal class AppManifestLoader
	{
		private enum ESteps
		{
			LoadStaticVersion,
			CheckStaticVersion,
			LoadAppManifest,
			CheckAppManifest,
			Succeed,
			Failed,
		}

		private ESteps _steps = ESteps.LoadStaticVersion;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;
		private int _staticVersion = 0;

		/// <summary>
		/// 错误日志
		/// </summary>
		public string Error { private set; get; }

		/// <summary>
		/// 补丁清单
		/// </summary>
		public PatchManifest Result { private set; get; }

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone()
		{
			if (_steps == ESteps.Succeed || _steps == ESteps.Failed)
				return true;
			else
				return false;
		}

		public void Update()
		{
			if (IsDone())
				return;

			if (_steps == ESteps.LoadStaticVersion)
			{
				YooLogger.Log($"Load application static version.");
				string filePath = PathHelper.MakeStreamingLoadPath(YooAssetSettings.VersionFileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(url);
				_steps = ESteps.CheckStaticVersion;
			}

			if (_steps == ESteps.CheckStaticVersion)
			{
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					Error = _downloader1.GetError();
					_steps = ESteps.Failed;
				}
				else
				{
					_staticVersion = int.Parse(_downloader1.GetText());
					_steps = ESteps.LoadAppManifest;
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.LoadAppManifest)
			{
				YooLogger.Log($"Load application patch manifest.");
				string filePath = PathHelper.MakeStreamingLoadPath(YooAssetSettingsData.GetPatchManifestFileName(_staticVersion));
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader2 = new UnityWebDataRequester();
				_downloader2.SendRequest(url);
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					Error = _downloader2.GetError();
					_steps = ESteps.Failed;
				}
				else
				{
					// 解析APP里的补丁清单
					Result = PatchManifest.Deserialize(_downloader2.GetText());
					_steps = ESteps.Succeed;
				}
				_downloader2.Dispose();
			}
		}
	}
}