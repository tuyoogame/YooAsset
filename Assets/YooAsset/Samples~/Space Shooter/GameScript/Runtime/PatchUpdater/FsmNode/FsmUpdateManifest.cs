using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

/// <summary>
/// 更新补丁清单
/// </summary>
public class FsmUpdateManifest : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmUpdateManifest);

	void IFsmNode.OnEnter()
	{
		Debug.Log("开始更新资源版本清单！");
		PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.UpdateManifest);
		GameBoot.Instance.StartCoroutine(UpdateManifest());
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}

	private IEnumerator UpdateManifest()
	{
		yield return new WaitForSecondsRealtime(0.5f);

		var package = YooAssets.GetAssetsPackage("DefaultPackage");
		var operation = package.UpdatePackageManifestAsync(PatchManager.PackageVersion, 30);
		yield return operation;

		if(operation.Status == EOperationStatus.Succeed)
		{
			FsmManager.Transition(nameof(FsmCreateDownloader));
		}
		else
		{
			Debug.LogWarning(operation.Error);
			PatchEventDispatcher.SendPatchManifestUpdateFailedMsg();
		}
	}
}