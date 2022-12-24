using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 检查包裹内容的完整性
	/// </summary>
	public abstract class CheckContentsIntegrityOperation : AsyncOperationBase
	{
	}

	internal sealed class EditorSimulateModeCheckContentsIntegrityOperation : CheckContentsIntegrityOperation
	{
		internal EditorSimulateModeCheckContentsIntegrityOperation()
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
	internal sealed class OfflinePlayModeCheckContentsIntegrityOperation : CheckContentsIntegrityOperation
	{
		internal OfflinePlayModeCheckContentsIntegrityOperation()
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
	internal sealed class HostPlayModeCheckContentsIntegrityOperation : CheckContentsIntegrityOperation
	{
		private enum ESteps
		{
			None,
			CheckLoadedManifest,
			VerifyPackage,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private VerifyPackageOperation _verifyOperation;
		private ESteps _steps = ESteps.None;

		internal HostPlayModeCheckContentsIntegrityOperation(HostPlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.CheckLoadedManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CheckLoadedManifest)
			{
				if (_impl.ActiveManifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found loaded package : {_packageName}";
				}
				else
				{
					_steps = ESteps.VerifyPackage;
				}
			}

			if (_steps == ESteps.VerifyPackage)
			{
				if (_verifyOperation == null)
				{
					_verifyOperation = VerifyPackageOperation.CreateOperation(_impl.ActiveManifest, _impl);
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