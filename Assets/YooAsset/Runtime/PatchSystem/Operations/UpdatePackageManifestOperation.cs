using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 向远端请求并更新补丁清单
	/// </summary>
	public abstract class UpdatePackageManifestOperation : AsyncOperationBase
	{
		/// <summary>
		/// 发现了新的清单
		/// </summary>
		public bool FoundNewManifest { protected set; get; } = false;

		/// <summary>
		/// 手动保存清单文件
		/// </summary>
		public virtual void SaveManifestFile() { }

		/// <summary>
		/// 还原补丁清单
		/// </summary>
		public virtual void RevertManifest() { }
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public EditorPlayModeUpdatePackageManifestOperation()
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
	/// 离线模式的更新清单操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		public OfflinePlayModeUpdatePackageManifestOperation()
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
	/// 联机模式的更新清单操作
	/// 注意：优先比对沙盒清单哈希值，如果有变化就更新远端清单文件，并保存到本地。
	/// </summary>
	internal sealed class HostPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
	{
		private enum ESteps
		{
			None,
			TryLoadCacheHash,
			DownloadWebHash,
			CheckDownloadWebHash,
			DownloadWebManifest,
			CheckDownloadWebManifest,
			CheckDeserializeWebManifest,
			StartVerifyOperation,
			CheckVerifyOperation,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly bool _autoSaveManifestFile;
		private readonly int _timeout;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;
		private DeserializeManifestOperation _deserializer;
		private VerifyCacheFilesOperation _verifyOperation;

		internal PatchManifest _prePatchManifest;
		private string _cacheManifestHash;
		private byte[] _fileBytesData = null;
		private ESteps _steps = ESteps.None;

		internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, bool autoSaveManifestFile, int timeout)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_autoSaveManifestFile = autoSaveManifestFile;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.TryLoadCacheHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.TryLoadCacheHash)
			{
				string filePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
				if (File.Exists(filePath))
				{
					_cacheManifestHash = HashUtility.FileMD5(filePath);
					_steps = ESteps.DownloadWebHash;
				}
				else
				{
					_steps = ESteps.DownloadWebManifest;
				}
			}

			if (_steps == ESteps.DownloadWebHash)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestHashFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckDownloadWebHash;
			}

			if (_steps == ESteps.CheckDownloadWebHash)
			{
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader1.GetError();
				}
				else
				{
					string webManifestHash = _downloader1.GetText();
					if (_cacheManifestHash == webManifestHash)
					{
						YooLogger.Log($"Not found new package : {_packageName}");
						FoundNewManifest = false;
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
					else
					{
						YooLogger.Log($"Package {_packageName} is change : {_cacheManifestHash} -> {webManifestHash}");
						FoundNewManifest = true;
						_steps = ESteps.DownloadWebManifest;
					}
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.DownloadWebManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_packageName, _packageVersion);
				string webURL = GetPatchManifestRequestURL(fileName);
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloader2 = new UnityWebDataRequester();
				_downloader2.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckDownloadWebManifest;
			}

			if (_steps == ESteps.CheckDownloadWebManifest)
			{
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader2.GetError();
				}
				else
				{
					byte[] bytesData = _downloader2.GetData();
					if (_autoSaveManifestFile)
					{
						SaveManifestFileInternal(bytesData);
					}
					else
					{
						_fileBytesData = bytesData;
					}

					// 解析二进制数据
					_deserializer = new DeserializeManifestOperation(bytesData);
					OperationSystem.StartOperation(_deserializer);
					_steps = ESteps.CheckDeserializeWebManifest;
				}

				_downloader2.Dispose();
			}

			if (_steps == ESteps.CheckDeserializeWebManifest)
			{
				Progress = _deserializer.Progress;
				if (_deserializer.IsDone == false)
					return;

				if (_deserializer.Status == EOperationStatus.Succeed)
				{
					_prePatchManifest = _impl.ActivePatchManifest;
					_impl.ActivePatchManifest = _deserializer.Manifest;
					_steps = ESteps.StartVerifyOperation;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _deserializer.Error;
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
				_verifyOperation = VerifyCacheFilesOperation.CreateOperation(_deserializer.Manifest, _impl);
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

		/// <summary>
		/// 手动保存清单文件
		/// </summary>
		public override void SaveManifestFile()
		{
			if (IsDone == false)
			{
				YooLogger.Warning($"{nameof(UpdatePackageManifestOperation)} is not done !");
				return;
			}

			if (Status == EOperationStatus.Succeed)
			{
				if (_fileBytesData != null)
				{
					SaveManifestFileInternal(_fileBytesData);
					_fileBytesData = null;
				}
			}
		}

		/// <summary>
		/// 还原补丁清单
		/// </summary>
		public override void RevertManifest()
		{
			if (IsDone == false)
			{
				YooLogger.Warning($"{nameof(UpdatePackageManifestOperation)} is not done !");
				return;
			}

			if (Status == EOperationStatus.Succeed)
			{
				if (_prePatchManifest != null)
				{
					_impl.ActivePatchManifest = _prePatchManifest;
					_prePatchManifest = null;
				}
			}
		}

		private void SaveManifestFileInternal(byte[] bytesData)
		{
			string savePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
			FileUtility.CreateFile(savePath, bytesData);
		}
		private string GetPatchManifestRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return _impl.GetPatchDownloadFallbackURL(fileName);
			else
				return _impl.GetPatchDownloadMainURL(fileName);
		}
	}
}