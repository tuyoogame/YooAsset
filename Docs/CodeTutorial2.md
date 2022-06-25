# 资源更新

**获取资源版本**

对于联机运行模式，在更新补丁清单之前，需要获取一个资源版本号。

该资源版本号，可以通过YooAssets提供的接口来更新，也可以通过HTTP访问游戏服务器来获取。

````c#
private IEnumerator UpdateStaticVersion()
{
    UpdateStaticVersionOperation operation = YooAssets.UpdateStaticVersionAsync();
    yield return operation;

    if (operation.Status == EOperationStatus.Succeed)
    {
        //更新成功
        int resourceVersion = operation.ResourceVersion;
        Debug.Log($"Update resource Version : {resourceVersion}");
    }
    else
    {
        //更新失败
        Debug.LogError(operation.Error);
    }
}
````

**更新补丁清单**

对于联机运行模式，在获取到资源版本号之后，就可以更新资源清单了。

````c#
private IEnumerator UpdatePatchManifest()
{
    UpdateManifestOperation operation = YooAssets.UpdateManifestAsync(resourceVersion);
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

- YooAssets.CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)

  用于下载更新当前资源版本所有的资源包文件。

- YooAssets.CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)

  用于下载更新资源标签指定的资源包文件。

- YooAssets.CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)

  用于下载更新指定的资源列表依赖的资源包文件。

````c#
IEnumerator Download()
{
    int downloadingMaxNum = 10;
    int failedTryAgain = 3;
    DownloaderOperation downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
    
    //没有需要下载的资源
    if (downloader.TotalDownloadCount == 0)
    {        
        yield break;
    }

    //需要下载的文件总数和总大小
    int totalDownloadCount = downloader.TotalDownloadCount;
    long totalDownloadBytes = downloader.TotalDownloadBytes;    

    //注册回调方法
    downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
    downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
    downloader.OnDownloadOverCallback = OnDownloadOverFunction;
    downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

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



