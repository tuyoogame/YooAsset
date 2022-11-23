using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class FsmClearCache : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmClearCache);

	void IFsmNode.OnEnter()
	{
		Debug.Log("开始清理未使用的缓存文件！");
		var package = YooAsset.YooAssets.GetAssetsPackage("DefaultPackage");
		var operation = package.ClearPackageUnusedCacheFilesAsync();
		operation.Completed += Operation_Completed;
	}

	private void Operation_Completed(YooAsset.AsyncOperationBase obj)
	{
		FsmManager.Transition(nameof(FsmStartGame));
	}

	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}
}