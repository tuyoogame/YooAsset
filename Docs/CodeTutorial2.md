# 资源更新

**更新补丁清单**

对于联机运行模式，在初始化资源系统之后，需要立刻更新资源清单。

**注意**：在初始化资源系统的时候，可以选择是否忽略资源版本号，这会影响到我们的更新步骤。

- 没有忽略资源版本号：在更新之前先获取更新的资源版本号，一般通过HTTP访问游戏服务器来获取。
- 忽略资源版本号：在更新的时候，资源版本号可以设置为0。

````c#
private IEnumerator UpdatePatchManifest()
{
    UpdateManifestOperation operation = YooAssets.UpdateManifestAsync(updateResourceVersion);
    yield return operation;

    if (operation.Status == EOperationStatus.Succeed)
    {
        //更新成功
    }
    else
    {
        //更新失败
        Debug.LogError(operation.Error);
    }
}
````

**补丁包下载**

在补丁清单更新完毕后，就可以更新资源文件了。

根据产品需求，可以选择更新全部资源，或者只更新部分资源。

补丁包下载接口：

- YooAssets.CreatePatchDownloader(string[] tags) 根据资源标签列表下载相关资源包文件
- YooAssets.CreateBundleDownloader(string[] locations) 根据资源对象列表下载相关资源包文件

````c#
IEnumerator Download()
{
    string[] tags = { "buildin", "config" };
    int downloadingMaxNum = 10;
    int failedTryAgain = 3;
    DownloaderOperation downloader = YooAssets.CreatePatchDownloader(tags, downloadingMaxNum, failedTryAgain);
    
    //没有需要下载的资源
    if (downloader.TotalDownloadCount == 0)
    {        
        yield break;
    }

    //需要下载的文件总数和总大小
    int totalDownloadCount = downloader.TotalDownloadCount;
    long totalDownloadBytes = downloader.TotalDownloadBytes;    

    //注册回调方法
    downloader.OnDownloadFileFailedCallback = OneDownloadFileFailed;
    downloader.OnDownloadProgressCallback = OnDownloadProgressUpdate;
    downloader.OnDownloadOverCallback = OnDownloadOver;

    //开启下载
    downloader.BeginDownload();
    yield return downloader;

    //检测下载结果
    if (downloader.Status == EOperationStatus.Succeed)
    {
        //下载成功
    }
    else
    {
        //下载失败
    }
}
````



