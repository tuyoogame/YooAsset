using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Window;
using UniFramework.Event;
using UniFramework.Machine;
using UniFramework.Module;
using YooAsset;

internal class FsmSceneBattle : IStateNode
{
	private BattleRoom _battleRoom;

	void IStateNode.OnCreate(StateMachine machine)
	{	
	}
	void IStateNode.OnEnter()
	{
		UniModule.StartCoroutine(Prepare());
	}
	void IStateNode.OnUpdate()
	{
		if(_battleRoom != null)
			_battleRoom.UpdateRoom();
	}
	void IStateNode.OnExit()
	{
		if(_battleRoom != null)
		{
			_battleRoom.DestroyRoom();
			_battleRoom = null;
		}
	}

	private IEnumerator Prepare()
	{
		yield return UniWindow.OpenWindowAsync<UILoadingWindow>("UILoading");
		yield return YooAssets.LoadSceneAsync("scene_battle");

		_battleRoom = new BattleRoom();
		yield return _battleRoom.LoadRoom();

		// 等所有数据准备完毕后，关闭加载界面。
		UniWindow.CloseWindow<UILoadingWindow>();
	}
}