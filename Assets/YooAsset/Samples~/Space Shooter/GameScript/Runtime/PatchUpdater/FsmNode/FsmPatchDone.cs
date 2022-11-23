using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 更新完毕
/// </summary>
internal class FsmPatchDone : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmPatchDone);

	void IFsmNode.OnEnter()
	{
		Debug.Log("补丁流程更新完毕！");
		PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.PatchDone);
		FsmManager.Transition(nameof(FsmClearCache));
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}
}