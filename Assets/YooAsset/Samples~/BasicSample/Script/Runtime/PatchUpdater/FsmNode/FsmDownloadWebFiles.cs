using System.Collections;
using YooAsset;

public class FsmDownloadWebFiles : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmDownloadWebFiles);

	void IFsmNode.OnEnter()
	{
		PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.DownloadWebFiles);
		BootScene.Instance.StartCoroutine(BeginDownload());
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}

	private IEnumerator BeginDownload()
	{
		var downloader = PatchUpdater.Downloader;

		// 注册下载回调
		downloader.OnDownloadErrorCallback = PatchEventDispatcher.SendWebFileDownloadFailedMsg;
		downloader.OnDownloadProgressCallback = PatchEventDispatcher.SendDownloadProgressUpdateMsg;
		downloader.BeginDownload();
		yield return downloader;

		// 检测下载结果
		if (downloader.Status != EOperationStatus.Succeed)
			yield break;

		FsmManager.Transition(nameof(FsmPatchDone));
	}
}