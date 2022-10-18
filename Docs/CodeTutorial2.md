# 资源更新

**获取资源版本**

对于联机运行模式，在更新补丁清单之前，需要获取一个资源版本。

该资源版本可以通过YooAssets提供的接口来更新，也可以通过HTTP访问游戏服务器来获取。

````c#
private IEnumerator UpdateStaticVersion()
{
    var package = YooAssets.GetAssetsPackage("DefaultPackage");
    UpdateStaticVersionOperation operation = package.UpdateStaticVersionAsync();
    yield return operation;

    if (operation.Status == EOperationStatus.Succeed)
    {
        //更新成功
        string packageCRC = operation.PackageCRC;
        Debug.Log($"Update resource Version : {packageCRC}");
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
    var package = YooAssets.GetAssetsPackage("DefaultPackage");
    UpdateManifestOperation operation = package.UpdateManifestAsync(packageCRC);
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

- YooAssets.CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout)

  用于下载更新当前资源版本所有的资源包文件。

- YooAssets.CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)

  用于下载更新资源标签指定的资源包文件。

- YooAssets.CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout)

  用于下载更新指定的资源列表依赖的资源包文件。

````c#
IEnumerator Download()
{
    int downloadingMaxNum = 10;
    int failedTryAgain = 3;
    int timeout = 60;
    var downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain, timeout);
    
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

**弱联网更新解决方案**

对于偏单机但是也有资源热更需求的项目。当玩家本地网络不稳定或无网络的时候，我们又不希望玩家卡在资源更新步骤而不能正常游戏。所以当玩家本地网络有问题的时候，我们可以跳过资源更新的步骤。

````c#
private IEnumerator UpdateStaticVersion()
{
    var package = YooAssets.GetAssetsPackage("DefaultPackage");
    UpdateStaticVersionOperation operation = package.UpdateStaticVersionAsync(10);
    yield return operation;
    if (operation.Status == EOperationStatus.Succeed)
    {
        // 如果获取远端资源版本成功，说明当前网络连接并无问题，可以走正常更新流程。
        ......
        
        // 注意：在成功下载所有资源之后，我们需要记录当前最新的资源版本号
        PlayerPrefs.SetString("STATIC_VERSION", packageCRC);
    }
    else
    {
        // 如果获取远端资源版本失败，我们走弱联网更新模式。
        // 注意：如果从来没有保存过版本信息，则需要从内部读取StaticVersion.bytes文件的版本信息。
        string packageCRC = PlayerPrefs.GetString("STATIC_VERSION", string.Empty);
        if (packageCRC == string.Empty)
        {
            packageCRC = LoadStaticVersionFromStreamingAssets();
        }
        
        // 在弱联网情况下更新补丁清单
        UpdateManifestOperation operation2 = package.WeaklyUpdateManifestAsync(packageCRC);
        yield return operation2;
        if (operation2.Status == EOperationStatus.Succeed)
        {
            StartGame();
        }
        else
        {
            // 指定版本的资源内容本地并不完整，需要提示玩家更新。
            ShowMessageBox("请检查本地网络，有新的游戏内容需要更新！");
        }
    }
}
````

