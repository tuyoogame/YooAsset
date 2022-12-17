using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Module;
using YooAsset;

/// <summary>
/// 更新资源清单
/// </summary>
public class FsmUpdateManifest : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("更新资源清单！");
		UniModule.StartCoroutine(UpdateManifest());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private IEnumerator UpdateManifest()
	{
		yield return new WaitForSecondsRealtime(0.5f);

		var package = YooAssets.GetAssetsPackage("DefaultPackage");
		var operation = package.UpdatePackageManifestAsync(PatchManager.Instance.PackageVersion);
		yield return operation;

		if(operation.Status == EOperationStatus.Succeed)
		{
			_machine.ChangeState<FsmCreateDownloader>();
		}
		else
		{
			Debug.LogWarning(operation.Error);
			PatchEventDefine.PatchManifestUpdateFailed.SendEventMessage();
		}
	}
}