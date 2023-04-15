using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Singleton;
using YooAsset;

/// <summary>
/// 更新资源版本号
/// </summary>
internal class FsmUpdateVersion : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("获取最新的资源版本 !");
		UniSingleton.StartCoroutine(GetStaticVersion());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private IEnumerator GetStaticVersion()
	{
		yield return new WaitForSecondsRealtime(0.5f);

    //TODO wht real 以下代码全拿
		var package = YooAssets.GetPackage("DefaultPackage");
		var operation = package.UpdatePackageVersionAsync();
		yield return operation;

		if (operation.Status == EOperationStatus.Succeed)
		{
			PatchManager.Instance.PackageVersion = operation.PackageVersion;
			_machine.ChangeState<FsmUpdateManifest>();
		}
		else
		{
			Debug.LogWarning(operation.Error);

			// 如果获取远端资源版本失败，说明当前网络无连接。
			// 在正常开始游戏之前，需要验证本地清单内容的完整性。
			string packageVersion = package.GetPackageVersion();
			var operation2 = package.CheckPackageContentsAsync(packageVersion);
			yield return operation2;
			if (operation2.Status == EOperationStatus.Succeed)
			{
				//  TODO wht real 可开始游戏
			}
			else
			{
				// TODO wht real资源内容本地并不完整，需要提示玩家联网更新。
			}

			//TODO wht real 重试
			PatchEventDefine.PackageVersionUpdateFailed.SendEventMessage();		//TODO wht real 不要
		}
	}
}