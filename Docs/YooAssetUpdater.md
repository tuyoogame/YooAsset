# 资源更新

**更新补丁清单**

对于联机运行模式，在初始化资源系统之后，需要立刻更新资源清单。

在此之前，需要获取更新的资源版本号，一般通过HTTP访问游戏服务器来获取。

注意：如果资源系统在初始化的时候，选择忽略资源版本号，那么更新的资源版本号可以设置为0。

````c#
private IEnumerator UpdatePatchManifest()
{
    int updateResourceVersion = 123;
    int timeout = 30;
    UpdateManifestOperation operation = YooAssets.UpdateManifestAsync(updateResourceVersion, timeout);
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

````c#
private DownloaderOperation _downloader;

/// <summary>
/// 创建下载器
/// </summary>
private void CreateDownloader()
{
    string[] tags = { "buildin", "config" };
    int downloadingMaxNum = 10;
    int failedTryAgain = 3;
    _downloader = YooAssets.CreatePatchDownloader(tags, downloadingMaxNum, failedTryAgain);
    if (_downloader.TotalDownloadCount == 0)
    {
        //没有需要下载的资源
    }
    else
    {
        //需要下载的文件总数和总大小
        int totalDownloadCount = _downloader.TotalDownloadCount;
        long totalDownloadBytes = _downloader.TotalDownloadBytes;
    }
}

/// <summary>
/// 开启下载
/// </summary>
private IEnumerator Download()
{
    //注册回调方法
    _downloader.OnDownloadFileFailedCallback = OneDownloadFileFailed;
    _downloader.OnDownloadProgressCallback = OnDownloadProgressUpdate;
    _downloader.OnDownloadOverCallback = OnDownloadOver;

    //开启下载
    _downloader.BeginDownload();
    yield return _downloader;

    //检测下载结果
    if (_downloader.Status == EOperationStatus.Succeed)
    {
        //下载成功
    }
    else
    {
        //下载失败
    }
}
````

