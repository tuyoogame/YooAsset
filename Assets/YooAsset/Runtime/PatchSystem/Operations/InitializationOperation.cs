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
		private string _simulatePatchManifestPath;
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
					Error = $"Manifest file not found : {_simulatePatchManifestPath}";
					return;
				}

				YooLogger.Log($"Load manifest file : {_simulatePatchManifestPath}");
				string jsonContent = FileUtility.ReadFile(_simulatePatchManifestPath);
				var simulatePatchManifest = PatchManifest.Deserialize(jsonContent);
				_impl.SetSimulatePatchManifest(simulatePatchManifest);
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
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
			QueryPackageVersion,
			LoadAppManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly OfflinePlayModeImpl _impl;
		private readonly string _packageName;
		private readonly CacheVerifier _patchCacheVerifier;
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
			_patchCacheVerifier = new CacheVerifierWithoutThread();
#else
			_patchCacheVerifier = new CacheVerifierWithThread();
#endif
		}
		internal override void Start()
		{
			_steps = ESteps.QueryPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.QueryPackageVersion)
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

				if (_appManifestLoader.Manifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
				}
				else
				{
					_steps = ESteps.InitVerifyingCache;
					_impl.SetAppPatchManifest(_appManifestLoader.Manifest);
				}
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				var verifyInfos = _impl.GetVerifyInfoList();
				_patchCacheVerifier.InitVerifier(verifyInfos);
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
					YooLogger.Log($"Verify result : Success {_patchCacheVerifier.VerifySuccessList.Count}, Fail {_patchCacheVerifier.VerifyFailList.Count}, Elapsed time {costTime} seconds");
				}
			}
		}
	}

	/// <summary>
	/// 联机运行模式的初始化操作
	/// </summary>
	internal sealed class HostPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			QueryPackageVersion,
			LoadAppManifest,
			CopyAppManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly CacheVerifier _patchCacheVerifier;
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
			_patchCacheVerifier = new CacheVerifierWithoutThread();
#else
			_patchCacheVerifier = new CacheVerifierWithThread();
#endif
		}
		internal override void Start()
		{
			_steps = ESteps.QueryPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.QueryPackageVersion)
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
				if(string.IsNullOrEmpty(error) == false)
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

				if (_appManifestLoader.Manifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
				}
				else
				{
					_steps = ESteps.InitVerifyingCache;
					_impl.SetLocalPatchManifest(_appManifestLoader.Manifest);
				}
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				var verifyInfos = _impl.GetVerifyInfoList(false);
				_patchCacheVerifier.InitVerifier(verifyInfos);
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
					YooLogger.Log($"Verify result : Success {_patchCacheVerifier.VerifySuccessList.Count}, Fail {_patchCacheVerifier.VerifyFailList.Count}, Elapsed time {costTime} seconds");
				}
			}
		}
	}


	// 内置补丁清单版本查询器
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
						Error = $"Buildin package version is empty !";
				}
				_steps = ESteps.Done;
				_downloader.Dispose();
			}
		}
	}

	// 内置补丁清单加载器
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
					Manifest = PatchManifest.Deserialize(_downloader.GetText());
				}
				_steps = ESteps.Done;
				_downloader.Dispose();
			}
		}
	}

	// 内置补丁清单复制器
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
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(_buildinPackageName, _buildinPackageVersion);
				string destFilePath = PathHelper.MakePersistentLoadPath(fileName);
				if (File.Exists(destFilePath))
				{
					_steps = ESteps.Done;
				}
				else
				{
					string sourceFilePath = PathHelper.MakeStreamingLoadPath(fileName);
					string url = PathHelper.ConvertToWWWPath(sourceFilePath);
					_downloader = new UnityWebFileRequester();
					_downloader.SendRequest(url, destFilePath);
					_steps = ESteps.CheckAppManifest;
				}
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