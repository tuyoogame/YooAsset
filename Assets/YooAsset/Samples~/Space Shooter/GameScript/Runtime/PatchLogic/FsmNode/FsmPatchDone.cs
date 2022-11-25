using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Module;

/// <summary>
/// 更新完毕
/// </summary>
internal class FsmPatchDone : IStateNode
{
	void IStateNode.OnCreate(StateMachine machine)
	{
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("开始游戏！");

		// 创建游戏管理器
		UniModule.CreateModule<GameManager>();

		// 开启游戏流程
		GameManager.Instance.Run();
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}
}