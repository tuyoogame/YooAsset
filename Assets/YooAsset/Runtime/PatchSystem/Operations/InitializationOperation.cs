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
				if (_deserializer.IsDone == false)
					return;

				if (_deserializer.Status == EOperationStatus.Succeed)
				{
					InitializedPackageVersion = _deserializer.Manifest.PackageVersion;
					_impl.ActivePatchManifest = _deserializer.Manifest;
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
		private QueryBuildinPackageVersionOperation _buildinPackageVersionQuery;
		private LoadBuildinManifestOperation _buildinManifestLoad;
		private VerifyCacheFilesOperation _verifyOperation;
		private ESteps _steps = ESteps.None;
	
		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
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
				if (_buildinPackageVersionQuery == null)
				{
					_buildinPackageVersionQuery = new QueryBuildinPackageVersionOperation(_packageName);
					OperationSystem.StartOperation(_buildinPackageVersionQuery);
				}

				if (_buildinPackageVersionQuery.IsDone == false)
					return;

				if (_buildinPackageVersionQuery.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.LoadBuildinManifest;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _buildinPackageVersionQuery.Error;
				}
			}

			if (_steps == ESteps.LoadBuildinManifest)
			{
				if (_buildinManifestLoad == null)
				{
					_buildinManifestLoad = new LoadBuildinManifestOperation(_packageName, _buildinPackageVersionQuery.Version);
					OperationSystem.StartOperation(_buildinManifestLoad);
				}

				Progress = _buildinManifestLoad.Progress;
				if (_buildinManifestLoad.IsDone == false)
					return;

				if (_buildinManifestLoad.Status == EOperationStatus.Succeed)
				{
					InitializedPackageVersion = _buildinManifestLoad.Manifest.PackageVersion;
					_impl.ActivePatchManifest = _buildinManifestLoad.Manifest;
					_steps = ESteps.StartVerifyOperation;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _buildinManifestLoad.Error;
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
				_verifyOperation = VerifyCacheFilesOperation.CreateOperation(_impl.ActivePatchManifest, _impl);
				OperationSystem.StartOperation(_verifyOperation);
				_steps = ESteps.CheckVerifyOperation;
			}

			if (_steps == ESteps.CheckVerifyOperation)
			{
				Progress = _verifyOperation.Progress;
				if (_verifyOperation.IsDone)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
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
		private QueryBuildinPackageVersionOperation _buildinPackageVersionQuery;
		private CopyBuildinManifestOperation _buildinManifestCopy;
		private LoadBuildinManifestOperation _buildinManifestLoad;
		private LoadCacheManifestOperation _cacheManifestLoad;
		private VerifyCacheFilesOperation _verifyOperation;
		private ESteps _steps = ESteps.None;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
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
				if (_cacheManifestLoad == null)
				{
					_cacheManifestLoad = new LoadCacheManifestOperation(_packageName);
					OperationSystem.StartOperation(_cacheManifestLoad);
				}

				if (_cacheManifestLoad.IsDone == false)
					return;

				if (_cacheManifestLoad.Status == EOperationStatus.Succeed)
				{
					InitializedPackageVersion = _cacheManifestLoad.Manifest.PackageVersion;
					_impl.ActivePatchManifest = _cacheManifestLoad.Manifest;
					_steps = ESteps.StartVerifyOperation;
				}
				else
				{
					_steps = ESteps.QueryBuildinPackageVersion;
				}
			}

			if (_steps == ESteps.QueryBuildinPackageVersion)
			{
				if (_buildinPackageVersionQuery == null)
				{
					_buildinPackageVersionQuery = new QueryBuildinPackageVersionOperation(_packageName);
					OperationSystem.StartOperation(_buildinPackageVersionQuery);
				}

				if (_buildinPackageVersionQuery.IsDone == false)
					return;

				// 注意：为了兼容MOD模式，初始化动态新增的包裹的时候，如果内置清单不存在也不需要报错！
				if (_buildinPackageVersionQuery.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.CopyBuildinManifest;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					string error = _buildinPackageVersionQuery.Error;
					YooLogger.Log($"Failed to load buildin package version file : {error}");
				}
			}

			if (_steps == ESteps.CopyBuildinManifest)
			{
				if (_buildinManifestCopy == null)
				{
					_buildinManifestCopy = new CopyBuildinManifestOperation(_packageName, _buildinPackageVersionQuery.Version);
					OperationSystem.StartOperation(_buildinManifestCopy);
				}

				Progress = _buildinManifestCopy.Progress;
				if (_buildinManifestCopy.IsDone == false)
					return;

				if (_buildinManifestCopy.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.LoadBuildinManifest;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _buildinManifestCopy.Error;
				}
			}

			if (_steps == ESteps.LoadBuildinManifest)
			{
				if (_buildinManifestLoad == null)
				{
					_buildinManifestLoad = new LoadBuildinManifestOperation(_packageName, _buildinPackageVersionQuery.Version);
					OperationSystem.StartOperation(_buildinManifestLoad);
				}

				Progress = _buildinManifestLoad.Progress;
				if (_buildinManifestLoad.IsDone == false)
					return;

				if (_buildinManifestLoad.Status == EOperationStatus.Succeed)
				{			
					InitializedPackageVersion = _buildinManifestLoad.Manifest.PackageVersion;
					_impl.ActivePatchManifest = _buildinManifestLoad.Manifest;
					_steps = ESteps.StartVerifyOperation;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _buildinManifestLoad.Error;
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
				_verifyOperation = VerifyCacheFilesOperation.CreateOperation(_impl.ActivePatchManifest, _impl);
				OperationSystem.StartOperation(_verifyOperation);
				_steps = ESteps.CheckVerifyOperation;
			}

			if (_steps == ESteps.CheckVerifyOperation)
			{
				Progress = _verifyOperation.Progress;
				if (_verifyOperation.IsDone)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
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
}