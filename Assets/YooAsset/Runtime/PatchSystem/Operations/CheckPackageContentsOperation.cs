using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 检查包裹内容的完整性
	/// </summary>
	public abstract class CheckPackageContentsOperation : AsyncOperationBase
	{
	}

	internal sealed class EditorSimulateModeCheckPackageContentsOperation : CheckPackageContentsOperation
	{
		internal EditorSimulateModeCheckPackageContentsOperation()
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
	internal sealed class OfflinePlayModeCheckPackageContentsOperation : CheckPackageContentsOperation
	{
		internal OfflinePlayModeCheckPackageContentsOperation()
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
	internal sealed class HostPlayModeCheckPackageContentsOperation : CheckPackageContentsOperation
	{
		private enum ESteps
		{
			None,
			CheckActiveManifest,
			LoadCacheManifest,
			VerifyPackage,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private LoadCacheManifestOperation _loadCacheManifestOp;
		private VerifyPackageOperation _verifyOperation;
		private PatchManifest _verifyManifest;
		private ESteps _steps = ESteps.None;

		internal HostPlayModeCheckPackageContentsOperation(HostPlayModeImpl impl, string packageName, string packageVersion)
		{
			_impl = impl;
			_packageName = packageName;
			_packageVersion = packageVersion;
		}
		internal override void Start()
		{
			_steps = ESteps.CheckActiveManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CheckActiveManifest)
			{
				// 检测当前激活的清单对象
				if (_impl.ActiveManifest != null && _impl.ActiveManifest.PackageVersion == _packageVersion)
				{
					_verifyManifest = _impl.ActiveManifest;
					_steps = ESteps.VerifyPackage;
				}
				else
				{
					_steps = ESteps.LoadCacheManifest;
				}
			}

			if (_steps == ESteps.LoadCacheManifest)
			{
				if (_loadCacheManifestOp == null)
				{
					_loadCacheManifestOp = new LoadCacheManifestOperation(_packageName, _packageVersion);
					OperationSystem.StartOperation(_loadCacheManifestOp);
				}

				if (_loadCacheManifestOp.IsDone == false)
					return;

				if (_loadCacheManifestOp.Status == EOperationStatus.Succeed)
				{
					_verifyManifest = _loadCacheManifestOp.Manifest;
					_steps = ESteps.VerifyPackage;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _loadCacheManifestOp.Error;
				}
			}

			if (_steps == ESteps.VerifyPackage)
			{
				if (_verifyOperation == null)
				{
					_verifyOperation = VerifyPackageOperation.CreateOperation(_verifyManifest, _impl);
					OperationSystem.StartOperation(_verifyOperation);
				}

				Progress = _verifyOperation.Progress;
				if (_verifyOperation.IsDone == false)
					return;

				bool verifySucceed = true;
				foreach (var verifyInfo in _verifyOperation.VerifyFailList)
				{
					// 注意：跳过内置资源文件
					if (verifyInfo.IsBuildinFile)
						continue;

					verifySucceed = false;
					YooLogger.Warning($"Failed verify file : {verifyInfo.VerifyFilePath}");
				}

				if (verifySucceed)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"The package resource {_packageName} content has verify failed file !";
				}
			}
		}
	}
}