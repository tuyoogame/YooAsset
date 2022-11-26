using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Window;
using UniFramework.Module;
using YooAsset;

internal class FsmSceneHome : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		UniModule.StartCoroutine(Prepare());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
		UniWindow.CloseWindow<UIHomeWindow>();
	}

	private IEnumerator Prepare()
	{
		if (_machine.PreviousNode != typeof(FsmInitGame).FullName)
			yield return  UniWindow.OpenWindowAsync<UILoadingWindow>("UILoading");

		yield return YooAssets.LoadSceneAsync("scene_home");	
		yield return UniWindow.OpenWindowAsync<UIHomeWindow>("UIHome");
		yield return new WaitForSeconds(0.5f);
		
		// 等所有数据准备完毕后，关闭加载界面。
		UniWindow.CloseWindow<UILoadingWindow>();
	}
}