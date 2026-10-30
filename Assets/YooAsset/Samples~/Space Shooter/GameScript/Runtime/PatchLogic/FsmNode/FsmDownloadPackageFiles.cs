using System.Collections;
using Framework.Machine;
using YooAsset;

internal class FsmDownloadPackageFiles : IState<PatchContext>
{
    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        PatchStepChangedEvent.SendEventMessage("Downloading resource files.");
        GameManager.Instance.StartCoroutine(StartDownload(context));
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }

    private IEnumerator StartDownload(PatchContext context)
    {
        var downloader = context.Downloader;
        downloader.DownloadError += PatchWebFileDownloadFailedEvent.SendEventMessage;
        downloader.DownloadProgressChanged += PatchDownloadUpdatedEvent.SendEventMessage;
        downloader.StartDownload();
        yield return downloader;

        // Check download result.
        if (downloader.Status != EOperationStatus.Succeeded)
            yield break;

        context.Machine.RequestChange(PatchState.DownloadPackageOver);
    }
}
