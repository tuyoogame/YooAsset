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
		/// <summary>
		/// 初始化内部加载的包裹版本
		/// </summary>
		public string InitializedPackageVersion;
	}

	/// <summary>
	/// 编辑器下模拟模式的初始化操作
	/// </summary>
	internal sealed class EditorSimulateModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			Load,
			Done,
		}

		private readonly EditorSimulateModeImpl _impl;
		private readonly string _simulatePatchManifestPath;
		private ESteps _steps = ESteps.None;

		internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, string simulatePatchManifestPath)
		{
			_impl = impl;
			_simulatePatchManifestPath = simulatePatchManifestPath;
		}
		internal override void Start()
		{
			_steps = ESteps.Load;
		}
		internal override void Update()
		{
			if (_steps == ESteps.Load)
			{
				if (File.Exists(_simulatePatchManifestPath) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found simulation manifest file : {_simulatePatchManifestPath}";
					return;
				}

				try
				{
					YooLogger.Log($"Load simulation manifest file : {_simulatePatchManifestPath}");
					string jsonContent = FileUtility.ReadFile(_simulatePatchManifestPath);
					var manifest = PatchManifest.Deserialize(jsonContent);
					InitializedPackageVersion = manifest.PackageVersion;
					_impl.SetSimulatePatchManifest(manifest);
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				catch (System.Exception e)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = e.Message;
				}
			}
		}
	}

	/// <summary>
	/// 离线运行模式的初始化操作
	/// </summary>
	internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			QueryAppPackageVersion,
			LoadAppManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly OfflinePlayModeImpl _impl;
		private readonly string _packageName;
		private readonly CacheVerifier _cacheVerifier;
		private readonly AppPackageVersionQuerier _appPackageVersionQuerier;
		private AppManifestLoader _appManifestLoader;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
			_appPackageVersionQuerier = new AppPackageVersionQuerier(packageName);

#if UNITY_WEBGL
			_cacheVerifier = new CacheVerifierWithoutThread();
#else
			_cacheVerifier = new CacheVerifierWithThread();
#endif
		}
		internal override void Start()
		{
			_steps = ESteps.QueryAppPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.QueryAppPackageVersion)
			{
				_appPackageVersionQuerier.Update();
				if (_appPackageVersionQuerier.IsDone == false)
					return;

				string error = _appPackageVersionQuerier.Error;
				if (string.IsNullOrEmpty(error) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = error;
				}
				else
				{
					_appManifestLoader = new AppManifestLoader(_packageName, _appPackageVersionQuerier.Version);
					_steps = ESteps.LoadAppManifest;
				}
			}

			if (_steps == ESteps.LoadAppManifest)
			{
				_appManifestLoader.Update();
				Progress = _appManifestLoader.Progress;
				if (_appManifestLoader.IsDone == false)
					return;

				var manifest = _appManifestLoader.Manifest;
				if (manifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
				}
				else
				{
					InitializedPackageVersion = manifest.PackageVersion;
					_impl.SetAppPatchManifest(manifest);
					_steps = ESteps.InitVerifyingCache;
				}
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				var verifyInfos = _impl.GetVerifyInfoList();
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
	}

	/// <summary>
	/// 联机运行模式的初始化操作
	/// 注意：优先从沙盒里加载清单，如果沙盒里不存在就尝试把内置清单拷贝到沙盒并加载该清单。
	/// </summary>
	internal sealed class HostPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			TryLoadCacheManifest,
			QueryAppPackageVersion,
			CopyAppManifest,
			LoadAppManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly CacheVerifier _cacheVerifier;
		private readonly AppPackageVersionQuerier _appPackageVersionQuerier;
		private AppManifestCopyer _appManifestCopyer;
		private AppManifestLoader _appManifestLoader;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
			_appPackageVersionQuerier = new AppPackageVersionQuerier(packageName);

#if UNITY_WEBGL
			_cacheVerifier = new CacheVerifierWithoutThread();
#else
			_cacheVerifier = new CacheVerifierWithThread();
#endif
		}
		internal override void Start()
		{
			_steps = ESteps.TryLoadCacheManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.TryLoadCacheManifest)
			{
				if (PersistentHelper.CheckCacheManifestFileExists(_packageName))
				{
					try
					{
						var manifest = PersistentHelper.LoadCacheManifestFile(_packageName);
						InitializedPackageVersion = manifest.PackageVersion;
						_impl.SetLocalPatchManifest(manifest);
						_steps = ESteps.InitVerifyingCache;
					}
					catch (System.Exception e)
					{
						// 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，我们主动把损坏的文件删除。
						YooLogger.Warning($"Failed to load cache manifest file : {e.Message}");
						PersistentHelper.DeleteCacheManifestFile(_packageName);
						_steps = ESteps.QueryAppPackageVersion;
					}
				}
				else
				{
					_steps = ESteps.QueryAppPackageVersion;
				}
			}

			if (_steps == ESteps.QueryAppPackageVersion)
			{
				_appPackageVersionQuerier.Update();
				if (_appPackageVersionQuerier.IsDone == false)
					return;

				// 注意：为了兼容MOD模式，初始化动态新增的包裹的时候，如果内置清单不存在也不需要报错！
				string error = _appPackageVersionQuerier.Error;
				if (string.IsNullOrEmpty(error) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					YooLogger.Log($"Failed to load buildin package version file : {error}");
				}
				else
				{
					_appManifestCopyer = new AppManifestCopyer(_packageName, _appPackageVersionQuerier.Version);
					_appManifestLoader = new AppManifestLoader(_packageName, _appPackageVersionQuerier.Version);
					_steps = ESteps.CopyAppManifest;
				}
			}

			if (_steps == ESteps.CopyAppManifest)
			{
				_appManifestCopyer.Update();
				Progress = _appManifestCopyer.Progress;
				if (_appManifestCopyer.IsDone == false)
					return;

				string error = _appManifestCopyer.Error;
				if (string.IsNullOrEmpty(error) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = error;
				}
				else
				{
					_steps = ESteps.LoadAppManifest;
				}
			}

			if (_steps == ESteps.LoadAppManifest)
			{
				_appManifestLoader.Update();
				Progress = _appManifestLoader.Progress;
				if (_appManifestLoader.IsDone == false)
					return;

				var manifest = _appManifestLoader.Manifest;
				if (manifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
				}
				else
				{
					InitializedPackageVersion = manifest.PackageVersion;
					_impl.SetLocalPatchManifest(manifest);
					_steps = ESteps.InitVerifyingCache;
				}
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
	}


	/// <summary>
	/// 内置补丁清单版本查询器
	/// </summary>
	internal class AppPackageVersionQuerier
	{
		private enum ESteps
		{
			LoadStaticVersion,
			CheckStaticVersion,
			Done,
		}

		private readonly string _buildinPackageName;
		private ESteps _steps = ESteps.LoadStaticVersion;
		private UnityWebDataRequester _downloader;

		/// <summary>
		/// 内置包裹版本
		/// </summary>
		public string Version { private set; get; }

		/// <summary>
		/// 错误日志
		/// </summary>
		public string Error { private set; get; }

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _steps == ESteps.Done;
			}
		}


		public AppPackageVersionQuerier(string buildinPackageName)
		{
			_buildinPackageName = buildinPackageName;
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (IsDone)
				return;

			if (_steps == ESteps.LoadStaticVersion)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestVersionFileName(_buildinPackageName);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(url);
				_steps = ESteps.CheckStaticVersion;
			}

			if (_steps == ESteps.CheckStaticVersion)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
				}
				else
				{
					Version = _downloader.GetText();
					if (string.IsNullOrEmpty(Version))
						Error = $"Buildin package version file content is empty !";
				}
				_steps = ESteps.Done;
				_downloader.Dispose();
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
			LoadAppManifest,
			CheckAppManifest,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private ESteps _steps = ESteps.LoadAppManifest;
		private UnityWebDataRequester _downloader;

		/// <summary>
		/// 加载结果
		/// </summary>
		public PatchManifest Manifest { private set; get; }

		/// <summary>
		/// 错误日志
		/// </summary>
		public string Error { private set; get; }

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _steps == ESteps.Done;
			}
		}

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress
		{
			get
			{
				if (_downloader == null)
					return 0;
				return _downloader.Progress();
			}
		}


		public AppManifestLoader(string buildinPackageName, string buildinPackageVersion)
		{
			_buildinPackageName = buildinPackageName;
			_buildinPackageVersion = buildinPackageVersion;
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (IsDone)
				return;

			if (_steps == ESteps.LoadAppManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(url);
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
				}
				else
				{
					// 解析APP里的补丁清单
					try
					{
						Manifest = PatchManifest.Deserialize(_downloader.GetText());
					}
					catch (System.Exception e)
					{
						Error = e.Message;
					}
				}
				_steps = ESteps.Done;
				_downloader.Dispose();
			}
		}
	}

	/// <summary>
	/// 内置补丁清单复制器
	/// </summary>
	internal class AppManifestCopyer
	{
		private enum ESteps
		{
			CopyAppManifest,
			CheckAppManifest,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private ESteps _steps = ESteps.CopyAppManifest;
		private UnityWebFileRequester _downloader;

		/// <summary>
		/// 错误日志
		/// </summary>
		public string Error { private set; get; }

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _steps == ESteps.Done;
			}
		}

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress
		{
			get
			{
				if (_downloader == null)
					return 0;
				return _downloader.Progress();
			}
		}


		public AppManifestCopyer(string buildinPackageName, string buildinPackageVersion)
		{
			_buildinPackageName = buildinPackageName;
			_buildinPackageVersion = buildinPackageVersion;
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (IsDone)
				return;

			if (_steps == ESteps.CopyAppManifest)
			{
				string savePath = PersistentHelper.GetCacheManifestFilePath(_buildinPackageName);
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebFileRequester();
				_downloader.SendRequest(url, savePath);
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
				}
				_steps = ESteps.Done;
				_downloader.Dispose();
			}
		}
	}
}