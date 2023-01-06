using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Module;
using YooAsset;

/// <summary>
/// 更新资源版本号
/// </summary>
internal class FsmUpdateVersion : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("获取最新的资源版本 !");
		UniModule.StartCoroutine(GetStaticVersion());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private IEnumerator GetStaticVersion()
	{
		yield return new WaitForSecondsRealtime(0.5f);

		//TODO wht real 以下代码全拿
		var package = YooAssets.GetAssetsPackage("DefaultPackage");
		var operation = package.UpdatePackageVersionAsync();
		yield return operation;

		if (operation.Status == EOperationStatus.Succeed)
		{
			PatchManager.Instance.PackageVersion = operation.PackageVersion;
			_machine.ChangeState<FsmUpdateManifest>();
		}
		else
		{
			Debug.LogWarning(operation.Error);
			//TODO wht real 重试
			PatchEventDefine.PackageVersionUpdateFailed.SendEventMessage();		//TODO wht real 不要
		}
	}
}