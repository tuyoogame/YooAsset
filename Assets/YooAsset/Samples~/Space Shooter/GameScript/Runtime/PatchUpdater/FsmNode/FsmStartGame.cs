using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 开始游戏
/// </summary>
internal class FsmStartGame : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmStartGame);

	void IFsmNode.OnEnter()
	{
		Debug.Log("开始游戏！");
		YooAsset.YooAssets.LoadSceneAsync("scene_home");
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}
}