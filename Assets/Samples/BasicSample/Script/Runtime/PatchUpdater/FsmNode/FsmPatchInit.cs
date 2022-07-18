using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class FsmPatchInit : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmPatchInit);

	void IFsmNode.OnEnter()
	{
		// 加载更新面板
		var go = Resources.Load<GameObject>("PatchWindow");
		GameObject.Instantiate(go);

		BootScene.Instance.StartCoroutine(Begin());
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}

	private IEnumerator Begin()
	{
		yield return new WaitForSecondsRealtime(0.5f);

		FsmManager.Transition(nameof(FsmUpdateStaticVersion));
	}
}