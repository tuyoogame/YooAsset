using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初始化工作
/// </summary>
internal class FsmPatchInit : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmPatchInit);

	void IFsmNode.OnEnter()
	{
		// 加载更新面板
		var go = Resources.Load<GameObject>("PatchWindow");
		GameObject.Instantiate(go);

		FsmManager.Transition(nameof(FsmUpdateVersion));
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}
}