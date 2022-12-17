using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 检查本地包裹内容的完整性
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
			CheckLoadedManifest,
			StartVerifyOperation,
			CheckVerifyOperation,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private CacheFilesVerifyOperation _verifyOperation;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal HostPlayModeCheckPackageContentsOperation(HostPlayModeImpl impl, string packageName)
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
				if (_impl.ActivePatchManifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found loaded package : {_packageName}";
				}
				else
				{
					_steps = ESteps.StartVerifyOperation;
				}
			}

			if (_steps == ESteps.StartVerifyOperation)
			{
#if UNITY_WEBGL
				_verifyOperation = new CacheFilesVerifyWithoutThreadOperation(_impl.ActivePatchManifest, _impl);
#else
				_verifyOperation = new CacheFilesVerifyWithThreadOperation(_impl.ActivePatchManifest, _impl);
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
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_verifyOperation.VerifySuccessList.Count}, Fail {_verifyOperation.VerifyFailList.Count}, Elapsed time {costTime} seconds");

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
}