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
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly string _packageName;
		private readonly CacheVerifier _cacheVerifier;
		private ESteps _steps = ESteps.None;
		private float _verifyTime;

		internal HostPlayModeCheckPackageContentsOperation(HostPlayModeImpl impl, string packageName)
		{
			_impl = impl;
			_packageName = packageName;

#if UNITY_WEBGL
			_cacheVerifier = new CacheVerifierWithoutThread();
#else
			_cacheVerifier = new CacheVerifierWithThread();
#endif
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
				if (_impl.LocalPatchManifest == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found loaded package : {_packageName}";
				}
				else
				{
					_steps = ESteps.InitVerifyingCache;
				}
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				var verifyInfos = _impl.GetVerifyInfoList(true);
				_cacheVerifier.InitVerifier(verifyInfos);
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.UpdateVerifyingCache;
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _cacheVerifier.GetVerifierProgress();
				if (_cacheVerifier.UpdateVerifier())
				{
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_cacheVerifier.VerifySuccessList.Count}, Fail {_cacheVerifier.VerifyFailList.Count}, Elapsed time {costTime} seconds");

					bool verifySucceed = true;
					foreach (var verifyInfo in _cacheVerifier.VerifyFailList)
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