using UnityEngine;
using UniFramework.Machine;
using YooAsset;

internal class FsmCreateDownloader : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchStepChangedEvent.SendEventMessage("Creating resource downloader.");
        CreateDownloader();
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    void CreateDownloader()
    {
        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var options = new ResourceDownloaderOptions(downloadingMaxNum, failedTryAgain);
        var downloader = package.CreateResourceDownloader(options);
        _machine.SetBlackboardValue("Downloader", downloader);

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("No download files were found.");
            _machine.ChangeState<FsmStartGame>();
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