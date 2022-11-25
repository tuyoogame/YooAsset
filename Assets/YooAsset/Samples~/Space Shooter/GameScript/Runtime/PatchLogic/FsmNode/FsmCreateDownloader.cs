using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Module;
using YooAsset;

/// <summary>
/// 创建文件下载器
/// </summary>
public class FsmCreateDownloader : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("创建补丁下载器！");
		UniModule.StartCoroutine(CreateDownloader());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	IEnumerator CreateDownloader()
	{
		yield return new WaitForSecondsRealtime(0.5f);

		int downloadingMaxNum = 10;
		int failedTryAgain = 3;
		var downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
		PatchManager.Instance.Downloader = downloader;

		if (downloader.TotalDownloadCount == 0)
		{
			Debug.Log("没有发现需要下载的资源！");
			_machine.ChangeState<FsmDownloadOver>();
		}
		else
		{
			Debug.Log($"一共发现了{downloader.TotalDownloadCount}个资源需要更新下载！");

			// 发现新更新文件后，挂起流程系统
			// 注意：开发者需要在下载前检测磁盘空间不足
			int totalDownloadCount = downloader.TotalDownloadCount;
			long totalDownloadBytes = downloader.TotalDownloadBytes;
			PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
		}
	}
}