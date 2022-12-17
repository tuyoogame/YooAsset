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
			LoadManifestFileData,
			CheckDeserializeManifest,
			Done,
		}

		private readonly EditorSimulateModeImpl _impl;
		private readonly string _simulatePatchManifestPath;
		private DeserializeManifestOperation _deserializer;
		private ESteps _steps = ESteps.None;

		internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, string simulatePatchManifestPath)
		{
			_impl = impl;
			_simulatePatchManifestPath = simulatePatchManifestPath;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadManifestFileData;
		}
		internal override void Update()
		{
			if (_steps == ESteps.LoadManifestFileData)
			{
				if (File.Exists(_simulatePatchManifestPath) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found simulation manifest file : {_simulatePatchManifestPath}";
					return;
				}

				YooLogger.Log($"Load simulation manifest file : {_simulatePatchManifestPath}");
				byte[] bytesData = FileUtility.ReadAllBytes(_simulatePatchManifestPath);
				_deserializer = new DeserializeManifestOperation(bytesData);
				OperationSystem.StartOperation(_deserializer);
				_steps = ESteps.CheckDeserializeManifest;
			}

			if (_steps == ESteps.CheckDeserializeManifest)
			{
				if (_deserializer.IsDone)
				{
					if (_deserializer.Status == EOperationStatus.Succeed)
					{
						var manifest = _deserializer.Manifest;
						InitializedPackageVersion = manifest.PackageVersion;
						_impl.SetActivePatchManifest(manifest);
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
	}

	/// <summary>
	/// 离线运行模式的初始化操作
	/// </summary>
	internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			None,
			QueryBuildinPackageVersion,
			LoadBuildinManifest,
			StartVerifyOperation,
			CheckVerifyOperation,
			Done,
		}

		private readonly OfflinePlayModeImpl _impl;
		private readonly string _packageName;
		private readonly BuildinPackageVersionQuerier _buildinPackageVersionQuerier;
		private BuildinManifestLoader _buildinManifestLoader;
		private CacheFilesVerifyOperation _verifyOperation;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
			_buildinPackageVersionQuerier = new BuildinPackageVersionQuerier(packageName);
		}
		internal override void Start()
		{
			_steps = ESteps.QueryBuildinPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.QueryBuildinPackageVersion)
			{
				_buildinPackageVersionQuerier.Update();
				if (_buildinPackageVersionQuerier.IsDone == false)
					return;

				string error = _buildinPackageVersionQuerier.Error;
				if (string.IsNullOrEmpty(error) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = error;
				}
				else
				{
					_buildinManifestLoader = new BuildinManifestLoader(_packageName, _buildinPackageVersionQuerier.Version);
					_steps = ESteps.LoadBuildinManifest;
				}
			}

			if (_steps == ESteps.LoadBuildinManifest)
			{
				_buildinManifestLoader.Update();
				Progress = _buildinManifestLoader.Progress;
				if (_buildinManifestLoader.IsDone == false)
					return;

				var manifest = _buildinManifestLoader.Manifest;
				if (manifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _buildinManifestLoader.Error;
				}
				else
				{
					InitializedPackageVersion = manifest.PackageVersion;
					_impl.SetActivePatchManifest(manifest);
					_steps = ESteps.StartVerifyOperation;
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
#if UNITY_WEBGL
				_verifyOperation = new CacheFilesVerifyWithoutThreadOperation(_impl.AppPatchManifest, null);
#else
				_verifyOperation = new CacheFilesVerifyWithThreadOperation(_impl.ActivePatchManifest, null);
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
			CheckAppFootPrint,
			TryLoadCacheManifest,
			QueryBuildinPackageVersion,
			CopyBuildinManifest,
			LoadBuildinManifest,
			StartVerifyOperation,
			CheckVerifyOperation,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly BuildinPackageVersionQuerier _buildinPackageVersionQuerier;
		private BuildinManifestCopyer _buildinManifestCopyer;
		private BuildinManifestLoader _buildinManifestLoader;
		private CacheManifestLoader _cacheManifestLoader;
		private CacheFilesVerifyOperation _verifyOperation;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
			_buildinPackageVersionQuerier = new BuildinPackageVersionQuerier(packageName);
		}
		internal override void Start()
		{
			_steps = ESteps.CheckAppFootPrint;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CheckAppFootPrint)
			{
				var appFootPrint = new AppFootPrint();
				appFootPrint.Load();

				// 如果水印发生变化，则说明覆盖安装后首次打开游戏
				if (appFootPrint.IsDirty())
				{
					PersistentHelper.DeleteManifestFolder();
					appFootPrint.Coverage();
					YooLogger.Log("Delete manifest files when application foot print dirty !");
				}
				_steps = ESteps.TryLoadCacheManifest;
			}

			if (_steps == ESteps.TryLoadCacheManifest)
			{
				if (_cacheManifestLoader == null)
					_cacheManifestLoader = new CacheManifestLoader(_packageName);

				_cacheManifestLoader.Update();
				if (_cacheManifestLoader.IsDone)
				{
					var manifest = _cacheManifestLoader.Manifest;
					if (manifest != null)
					{
						InitializedPackageVersion = manifest.PackageVersion;
						_impl.SetActivePatchManifest(manifest);
						_steps = ESteps.StartVerifyOperation;
					}
					else
					{
						_steps = ESteps.QueryBuildinPackageVersion;
					}
				}
			}

			if (_steps == ESteps.QueryBuildinPackageVersion)
			{
				_buildinPackageVersionQuerier.Update();
				if (_buildinPackageVersionQuerier.IsDone == false)
					return;

				// 注意：为了兼容MOD模式，初始化动态新增的包裹的时候，如果内置清单不存在也不需要报错！
				string error = _buildinPackageVersionQuerier.Error;
				if (string.IsNullOrEmpty(error) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					YooLogger.Log($"Failed to load buildin package version file : {error}");
				}
				else
				{
					_buildinManifestCopyer = new BuildinManifestCopyer(_packageName, _buildinPackageVersionQuerier.Version);
					_buildinManifestLoader = new BuildinManifestLoader(_packageName, _buildinPackageVersionQuerier.Version);
					_steps = ESteps.CopyBuildinManifest;
				}
			}

			if (_steps == ESteps.CopyBuildinManifest)
			{
				_buildinManifestCopyer.Update();
				Progress = _buildinManifestCopyer.Progress;
				if (_buildinManifestCopyer.IsDone == false)
					return;

				string error = _buildinManifestCopyer.Error;
				if (string.IsNullOrEmpty(error) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = error;
				}
				else
				{
					_steps = ESteps.LoadBuildinManifest;
				}
			}

			if (_steps == ESteps.LoadBuildinManifest)
			{
				_buildinManifestLoader.Update();
				Progress = _buildinManifestLoader.Progress;
				if (_buildinManifestLoader.IsDone == false)
					return;

				var manifest = _buildinManifestLoader.Manifest;
				if (manifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _buildinManifestLoader.Error;
				}
				else
				{
					InitializedPackageVersion = manifest.PackageVersion;
					_impl.SetActivePatchManifest(manifest);
					_steps = ESteps.StartVerifyOperation;
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
#if UNITY_WEBGL
				_verifyOperation = new CacheFilesVerifyWithoutThreadOperation(_impl.LocalPatchManifest, _impl.QueryServices);
#else
				_verifyOperation = new CacheFilesVerifyWithThreadOperation(_impl.ActivePatchManifest, _impl.QueryServices);
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
	}


	/// <summary>
	/// 应用程序水印
	/// </summary>
	internal class AppFootPrint
	{
		private string _footPrint;

		/// <summary>
		/// 读取应用程序水印
		/// </summary>
		public void Load()
		{
			string footPrintFilePath = PersistentHelper.GetAppFootPrintFilePath();
			if (File.Exists(footPrintFilePath))
			{
				_footPrint = FileUtility.ReadAllText(footPrintFilePath);
			}
			else
			{
				Coverage();
			}
		}

		/// <summary>
		/// 检测水印是否发生变化
		/// </summary>
		public bool IsDirty()
		{
#if UNITY_EDITOR
			return _footPrint != Application.version;
#else
			return _footPrint != Application.buildGUID;
#endif
		}

		/// <summary>
		/// 覆盖掉水印
		/// </summary>
		public void Coverage()
		{
#if UNITY_EDITOR
			_footPrint = Application.version;
#else
			_footPrint = Application.buildGUID;
#endif
			string footPrintFilePath = PersistentHelper.GetAppFootPrintFilePath();
			FileUtility.CreateFile(footPrintFilePath, _footPrint);
			YooLogger.Log($"Save application foot print : {_footPrint}");
		}
	}

	/// <summary>
	/// 内置补丁清单版本查询器
	/// </summary>
	internal class BuildinPackageVersionQuerier
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


		public BuildinPackageVersionQuerier(string buildinPackageName)
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
	internal class BuildinManifestLoader
	{
		private enum ESteps
		{
			LoadBuildinManifest,
			CheckLoadBuildinManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private ESteps _steps = ESteps.LoadBuildinManifest;
		private UnityWebDataRequester _downloader;
		private DeserializeManifestOperation _deserializer;

		/// <summary>
		/// 加载结果
		/// </summary>
		public PatchManifest Manifest { private set; get; }

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress { private set; get; }

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


		public BuildinManifestLoader(string buildinPackageName, string buildinPackageVersion)
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

			if (_steps == ESteps.LoadBuildinManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(url);
				_steps = ESteps.CheckLoadBuildinManifest;
			}

			if (_steps == ESteps.CheckLoadBuildinManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
					_steps = ESteps.Done;
				}
				else
				{
					// 解析APP里的补丁清单
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
						Manifest = _deserializer.Manifest;
					}
					else
					{
						Error = _deserializer.Error;
					}
					_steps = ESteps.Done;
				}
			}
		}
	}

	/// <summary>
	/// 内置补丁清单复制器
	/// </summary>
	internal class BuildinManifestCopyer
	{
		private enum ESteps
		{
			CopyBuildinManifest,
			CheckCopyBuildinManifest,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private ESteps _steps = ESteps.CopyBuildinManifest;
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


		public BuildinManifestCopyer(string buildinPackageName, string buildinPackageVersion)
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

			if (_steps == ESteps.CopyBuildinManifest)
			{
				string savePath = PersistentHelper.GetCacheManifestFilePath(_buildinPackageName);
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebFileRequester();
				_downloader.SendRequest(url, savePath);
				_steps = ESteps.CheckCopyBuildinManifest;
			}

			if (_steps == ESteps.CheckCopyBuildinManifest)
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

	/// <summary>
	/// 沙盒补丁清单加载器
	/// </summary>
	internal class CacheManifestLoader
	{
		private enum ESteps
		{
			LoadCacheManifestFile,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string _packageName;
		private ESteps _steps = ESteps.LoadCacheManifestFile;
		private DeserializeManifestOperation _deserializer;
		private string _manifestFilePath;

		/// <summary>
		/// 加载结果
		/// </summary>
		public PatchManifest Manifest { private set; get; }

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress { private set; get; }

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


		public CacheManifestLoader(string packageName)
		{
			_packageName = packageName;
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (IsDone)
				return;

			if (_steps == ESteps.LoadCacheManifestFile)
			{
				_manifestFilePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
				if (File.Exists(_manifestFilePath) == false)
				{
					_steps = ESteps.Done;
					Error = $"Manifest file not found : {_manifestFilePath}";
					return;
				}

				byte[] bytesData = File.ReadAllBytes(_manifestFilePath);
				_deserializer = new DeserializeManifestOperation(bytesData);
				OperationSystem.StartOperation(_deserializer);
				_steps = ESteps.CheckDeserializeManifest;
			}

			if (_steps == ESteps.CheckDeserializeManifest)
			{
				Progress = _deserializer.Progress;
				if (_deserializer.IsDone)
				{
					if (_deserializer.Status == EOperationStatus.Succeed)
					{
						Manifest = _deserializer.Manifest;
					}
					else
					{
						Error = _deserializer.Error;

						// 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，我们主动把损坏的文件删除。
						if (File.Exists(_manifestFilePath))
						{
							YooLogger.Warning($"Failed to load cache manifest file : {Error}");
							YooLogger.Warning($"Invalid cache manifest file have been removed : {_manifestFilePath}");
							File.Delete(_manifestFilePath);
						}
					}
					_steps = ESteps.Done;
				}
			}
		}
	}
}