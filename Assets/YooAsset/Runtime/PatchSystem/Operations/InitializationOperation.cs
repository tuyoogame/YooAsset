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
			Update,
			Done,
		}

		private readonly OfflinePlayModeImpl _impl;
		private AppManifestLoader _appManifestLoader;
		private ESteps _steps = ESteps.None;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string buildinPackageName)
		{
			_impl = impl;
			_appManifestLoader = new AppManifestLoader(buildinPackageName);
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
				Progress = _appManifestLoader.Progress();
				if (_appManifestLoader.IsDone() == false)
					return;

				if (_appManifestLoader.Result == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _appManifestLoader.Error;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					_impl.SetAppPatchManifest(_appManifestLoader.Result);
				}
			}
		}
	}

	/// <summary>
	/// 联机运行模式的初始化操作
	/// </summary>
	internal sealed class HostPlayModeInitializationOperation : InitializationOperation
	{
		internal HostPlayModeInitializationOperation()
		{
		}
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
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
			Done,
		}

		private string _buildinPackageName;
		private ESteps _steps = ESteps.LoadStaticVersion;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;

		/// <summary>
		/// 错误日志
		/// </summary>
		public string Error { private set; get; }

		/// <summary>
		/// 加载结果
		/// </summary>
		public PatchManifest Result { private set; get; }

		/// <summary>
		/// 内置补丁清单CRC
		/// </summary>
		public string BuildinPackageCRC { private set; get; }


		public AppManifestLoader(string buildinPackageName)
		{
			_buildinPackageName = buildinPackageName;
		}

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone()
		{
			return _steps == ESteps.Done;
		}

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress()
		{
			if (_downloader2 == null)
				return 0;
			return _downloader2.Progress();
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (IsDone())
				return;

			if (_steps == ESteps.LoadStaticVersion)
			{
				string fileName = YooAssetSettingsData.GetStaticVersionFileName(_buildinPackageName);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(url);
				_steps = ESteps.CheckStaticVersion;
				YooLogger.Log($"Load static version file : {filePath}");
			}

			if (_steps == ESteps.CheckStaticVersion)
			{
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					Error = _downloader1.GetError();
					_steps = ESteps.Done;
				}
				else
				{
					BuildinPackageCRC = _downloader1.GetText();
					_steps = ESteps.LoadAppManifest;
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.LoadAppManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(_buildinPackageName, BuildinPackageCRC);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader2 = new UnityWebDataRequester();
				_downloader2.SendRequest(url);
				_steps = ESteps.CheckAppManifest;
				YooLogger.Log($"Load patch manifest file : {filePath}");
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					Error = _downloader2.GetError();
					_steps = ESteps.Done;
				}
				else
				{
					// 解析APP里的补丁清单
					Result = PatchManifest.Deserialize(_downloader2.GetText());
					_steps = ESteps.Done;
				}
				_downloader2.Dispose();
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

		private string _buildinPackageName;
		private string _buildinPackageCRC;
		private ESteps _steps = ESteps.CopyAppManifest;
		private UnityWebFileRequester _downloader1;

		/// <summary>
		/// 错误日志
		/// </summary>
		public string Error { private set; get; }

		/// <summary>
		/// 拷贝结果
		/// </summary>
		public bool Result { private set; get; }


		public AppManifestCopyer(string buildinPackageName, string buildinPackageCRC)
		{
			_buildinPackageName = buildinPackageName;
			_buildinPackageCRC = buildinPackageCRC;
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (IsDone())
				return;

			if (_steps == ESteps.CopyAppManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestFileName(_buildinPackageName, _buildinPackageCRC);
				string destFilePath = PathHelper.MakePersistentLoadPath(fileName);
				if (File.Exists(destFilePath))
				{
					Result = true;
					_steps = ESteps.Done;
					return;
				}
				else
				{
					YooLogger.Log($"Copy application patch manifest.");
					string sourceFilePath = PathHelper.MakeStreamingLoadPath(fileName);
					string url = PathHelper.ConvertToWWWPath(sourceFilePath);
					_downloader1 = new UnityWebFileRequester();
					_downloader1.SendRequest(url, destFilePath);
					_steps = ESteps.CheckAppManifest;
				}
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					Result = false;
					Error = _downloader1.GetError();
					_steps = ESteps.Done;
				}
				else
				{
					Result = true;
					_steps = ESteps.Done;
				}
				_downloader1.Dispose();
			}
		}

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone()
		{
			return _steps == ESteps.Done;
		}
	}
}