using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;
using UniFramework.Window;
using UniFramework.Machine;
using UniFramework.Module;
using YooAsset;

internal class FsmInitGame : IStateNode
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
	}

	private IEnumerator Prepare()
	{
		var handle = YooAssets.LoadAssetAsync<GameObject>("UICanvas");
		//TODO wht ref 参考；同步接口LoadAssetSync；关闭Enable Addressable，使用全路径
		// var handle = YooAssets.LoadAssetAsync<GameObject>("Assets/YooAsset/Assets/YooAsset/Samples/Space Shooter/GameRes/UIPanel/UICanvas.prefab");
		handle.Completed += (AssetOperationHandle handle) => 
		{
			//TODO wht ref 加载回调
		};
		yield return handle;	//TODO wht ref 也可以用携程来确定加载是否完成
		// handle.Release();	//TODO wht ref 释放资源		
		// var package = YooAssets.GetAssetsPackage("DefaultPackage");
    	// package.UnloadUnusedAssets();		//TODO wht ref 卸载引用计数为零的资源；切场景或按时间间隔来调

		var canvas = handle.InstantiateSync();		//TODO wht ref 参考；同步实例化游戏对象
		var desktop = canvas.transform.Find("Desktop").gameObject;
		GameObject.DontDestroyOnLoad(canvas);

		// 初始化窗口系统
		UniWindow.Initalize(desktop);

		// 初始化对象池系统
		UniPooling.Initalize();

		_machine.ChangeState<FsmSceneHome>();
	}
}