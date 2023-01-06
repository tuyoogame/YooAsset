using System.Collections;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Module;
using YooAsset;

/// <summary>
/// 下载更新文件
/// </summary>
public class FsmDownloadFiles : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("开始下载补丁文件！");
		UniModule.StartCoroutine(BeginDownload());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private IEnumerator BeginDownload()
	{
		var downloader = PatchManager.Instance.Downloader;

		//TODO wht real 以下代码全拿

		// 注册下载回调
		downloader.OnDownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
		downloader.OnDownloadProgressCallback = PatchEventDefine.DownloadProgressUpdate.SendEventMessage;
		// downloader.OnDownloadOverCallback = OnDownloadOverFunction;
    	// downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;
		downloader.BeginDownload();
		yield return downloader;

		// 检测下载结果
		if (downloader.Status == EOperationStatus.Succeed)
		{
			_machine.ChangeState<FsmPatchDone>();
			//TODO wht real 可以开始游戏
		}
		else
		{
			yield break;
		}
	}
}