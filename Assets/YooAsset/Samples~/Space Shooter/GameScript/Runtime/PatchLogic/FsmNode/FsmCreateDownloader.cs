using UnityEngine;
using Framework.Machine;
using YooAsset;

internal class FsmCreateDownloader : IState<PatchContext>
{
    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        PatchStepChangedEvent.SendEventMessage("Creating resource downloader.");
        CreateDownloader(context);
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }

    void CreateDownloader(PatchContext context)
    {
        var packageName = context.PackageName;
        var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var options = new ResourceDownloaderOptions(downloadingMaxNum, failedTryAgain);
        var downloader = package.CreateResourceDownloader(options);
        context.Downloader = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("No download files were found.");
            context.Machine.RequestChange(PatchState.StartGame);
        }
        else
        {
            // Suspend the patch workflow after update files are found.
            // Developers should check available disk space before downloading.
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;
            PatchFoundUpdateFilesEvent.SendEventMessage(totalDownloadCount, totalDownloadBytes);
        }
    }
}