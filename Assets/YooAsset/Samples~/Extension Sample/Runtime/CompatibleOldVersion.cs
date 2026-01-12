using System.Collections.Generic;
using UnityEngine;
using YooAsset;

#region InitializeParameters
/// <summary>
/// 初始化参数
/// </summary>
public abstract class InitializeParameters
{
    /// <summary>
    /// 同时加载Bundle文件的最大并发数
    /// </summary>
    public int BundleLoadingMaxConcurrency = int.MaxValue;

    /// <summary>
    /// 当资源引用计数为零的时候自动释放资源包
    /// </summary>
    public bool AutoUnloadBundleWhenUnused = false;

    /// <summary>
    /// WebGL平台强制同步加载资源对象
    /// </summary>
    public bool WebGLForceSyncLoadAsset = false;
}

/// <summary>
/// 编辑器下模拟运行模式的初始化参数
/// </summary>
public class EditorSimulateModeParameters : InitializeParameters
{
    public FileSystemParameters EditorFileSystemParameters;
}

/// <summary>
/// 离线运行模式的初始化参数
/// </summary>
public class OfflinePlayModeParameters : InitializeParameters
{
    public FileSystemParameters BuiltinFileSystemParameters;
}

/// <summary>
/// 联机运行模式的初始化参数
/// </summary>
public class HostPlayModeParameters : InitializeParameters
{
    public FileSystemParameters BuiltinFileSystemParameters;
    public FileSystemParameters CacheFileSystemParameters;
}

/// <summary>
/// WebGL运行模式的初始化参数
/// </summary>
public class WebPlayModeParameters : InitializeParameters
{
    public FileSystemParameters WebServerFileSystemParameters;
    public FileSystemParameters WebRemoteFileSystemParameters;
}
#endregion

#region InitializationOperation
public class InitializationOperation : AsyncOperationBase
{
    private bool _isDone = false;
    private readonly InitializePackageOperation _operation;

    internal InitializationOperation(InitializePackageOperation op)
    {
        _operation = op;
    }
    protected override void InternalStart()
    {
    }
    protected override void InternalUpdate()
    {
        if (_isDone)
            return;

        _operation.UpdateOperation();
        if (_operation.IsDone == false)
            return;

        _isDone = true;
        if (_operation.Status == EOperationStatus.Succeeded)
            SetResult();
        else
            SetError(_operation.Error);
    }
}
#endregion

#region DestroyOperation
public class DestroyOperation : AsyncOperationBase
{
    private bool _isDone = false;
    private readonly DestroyPackageOperation _operation;

    internal DestroyOperation(DestroyPackageOperation op)
    {
        _operation = op;
    }
    protected override void InternalStart()
    {
    }
    protected override void InternalUpdate()
    {
        if (_isDone)
            return;

        _operation.UpdateOperation();
        if (_operation.IsDone == false)
            return;

        _isDone = true;
        if (_operation.Status == EOperationStatus.Succeeded)
            SetResult();
        else
            SetError(_operation.Error);
    }
}
#endregion

#region UpdatePackageManifestOperation
public class UpdatePackageManifestOperation : AsyncOperationBase
{
    private bool _isDone = false;
    private readonly LoadPackageManifestOperation _operation;

    internal UpdatePackageManifestOperation(LoadPackageManifestOperation op)
    {
        _operation = op;
    }
    protected override void InternalStart()
    {
    }
    protected override void InternalUpdate()
    {
        if (_isDone)
            return;

        _operation.UpdateOperation();
        if (_operation.IsDone == false)
            return;

        _isDone = true;
        if (_operation.Status == EOperationStatus.Succeeded)
            SetResult();
        else
            SetError(_operation.Error);
    }
}
#endregion

#region ImportFileInfo
public struct ImportFileInfo
{
    /// <summary>
    /// 本地文件路径
    /// </summary>
    public string FilePath;

    /// <summary>
    /// 资源包名称
    /// </summary>
    public string BundleName;

    /// <summary>
    /// 资源包GUID
    /// </summary>
    public string BundleGuid;
}
#endregion

public static class CompatibleResourcePackage
{
    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static InitializationOperation InitializeAsync(this ResourcePackage package, InitializeParameters parameters)
    {
        if (parameters is EditorSimulateModeParameters)
        {
            var initializeParameters = parameters as EditorSimulateModeParameters;
            var options = new EditorSimulateModeOptions();
            options.BundleLoadingMaxConcurrency = initializeParameters.BundleLoadingMaxConcurrency;
            options.AutoUnloadBundleWhenUnused = initializeParameters.AutoUnloadBundleWhenUnused;
            options.WebGLForceSyncLoadAsset = initializeParameters.WebGLForceSyncLoadAsset;
            options.EditorFileSystemParameters = initializeParameters.EditorFileSystemParameters;
            var operation = package.InitializePackageAsync(options);
            var wrapper = new InitializationOperation(operation);
            AsyncOperationSystem.StartOperation(package.PackageName, wrapper);
            return wrapper;
        }
        else if (parameters is OfflinePlayModeParameters)
        {
            var initializeParameters = parameters as OfflinePlayModeParameters;
            var options = new OfflinePlayModeOptions();
            options.BundleLoadingMaxConcurrency = initializeParameters.BundleLoadingMaxConcurrency;
            options.AutoUnloadBundleWhenUnused = initializeParameters.AutoUnloadBundleWhenUnused;
            options.WebGLForceSyncLoadAsset = initializeParameters.WebGLForceSyncLoadAsset;
            options.BuiltinFileSystemParameters = initializeParameters.BuiltinFileSystemParameters;
            var operation = package.InitializePackageAsync(options);
            var wrapper = new InitializationOperation(operation);
            AsyncOperationSystem.StartOperation(package.PackageName, wrapper);
            return wrapper;
        }
        else if (parameters is HostPlayModeParameters)
        {
            var initializeParameters = parameters as HostPlayModeParameters;
            var options = new HostPlayModeOptions();
            options.BundleLoadingMaxConcurrency = initializeParameters.BundleLoadingMaxConcurrency;
            options.AutoUnloadBundleWhenUnused = initializeParameters.AutoUnloadBundleWhenUnused;
            options.WebGLForceSyncLoadAsset = initializeParameters.WebGLForceSyncLoadAsset;
            options.BuiltinFileSystemParameters = initializeParameters.BuiltinFileSystemParameters;
            options.CacheFileSystemParameters = initializeParameters.CacheFileSystemParameters;
            var operation = package.InitializePackageAsync(options);
            var wrapper = new InitializationOperation(operation);
            AsyncOperationSystem.StartOperation(package.PackageName, wrapper);
            return wrapper;
        }
        else if (parameters is WebPlayModeParameters)
        {
            var initializeParameters = parameters as WebPlayModeParameters;
            var options = new WebPlayModeOptions();
            options.BundleLoadingMaxConcurrency = initializeParameters.BundleLoadingMaxConcurrency;
            options.AutoUnloadBundleWhenUnused = initializeParameters.AutoUnloadBundleWhenUnused;
            options.WebGLForceSyncLoadAsset = initializeParameters.WebGLForceSyncLoadAsset;
            options.WebServerFileSystemParameters = initializeParameters.WebServerFileSystemParameters;
            options.WebRemoteFileSystemParameters = initializeParameters.WebRemoteFileSystemParameters;
            var operation = package.InitializePackageAsync(options);
            var wrapper = new InitializationOperation(operation);
            AsyncOperationSystem.StartOperation(package.PackageName, wrapper);
            return wrapper;
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static DestroyOperation DestroyAsync(this ResourcePackage package)
    {
        var operation = package.DestroyPackageAsync();
        var wrapper = new DestroyOperation(operation);
        AsyncOperationSystem.StartOperation(package.PackageName, wrapper);
        return wrapper;
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static RequestPackageVersionOperation RequestPackageVersionAsync(this ResourcePackage package, bool appendTimeTicks = true, int timeout = 60)
    {
        var options = new RequestPackageVersionOptions(appendTimeTicks, timeout);
        return package.RequestPackageVersionAsync(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static UpdatePackageManifestOperation UpdatePackageManifestAsync(this ResourcePackage package, string packageVersion, int timeout = 60)
    {
        var options = new LoadPackageManifestOptions(packageVersion, timeout);
        var operation = package.LoadPackageManifestAsync(options);
        var wrapper = new UpdatePackageManifestOperation(operation);
        AsyncOperationSystem.StartOperation(package.PackageName, wrapper);
        return wrapper;
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static PrefetchManifestOperation PreDownloadContentAsync(this ResourcePackage package, string packageVersion, int timeout = 60)
    {
        var options = new PrefetchManifestOptions(packageVersion, timeout);
        return package.PrefetchManifestAsync(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ClearCacheOperation ClearCacheFilesAsync(this ResourcePackage package, string fileClearMode, object clearParam = null)
    {
        var options = new ClearCacheOptions(fileClearMode, clearParam);
        return package.ClearCacheAsync(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync(this ResourcePackage package, int loopCount)
    {
        var options = new UnloadUnusedAssetsOptions(loopCount);
        return package.UnloadUnusedAssetsAsync(options);
    }

    #region 资源下载
    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateResourceDownloader(this ResourcePackage package, int downloadingMaxNumber, int failedTryAgain)
    {
        var options = new ResourceDownloaderOptions(downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateResourceDownloader(this ResourcePackage package, string tag, int downloadingMaxNumber, int failedTryAgain)
    {
        string[] tags = new string[] { tag };
        var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateResourceDownloader(this ResourcePackage package, string[] tags, int downloadingMaxNumber, int failedTryAgain)
    {
        var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, string location, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
    {
        var assetInfo = package.ConvertLocationToAssetInfo(location, null);
        var options = new BundleDownloaderOptions(assetInfo, recursiveDownload, downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, string location, int downloadingMaxNumber, int failedTryAgain)
    {
        return package.CreateBundleDownloader(location, false, downloadingMaxNumber, failedTryAgain);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, string[] locations, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
    {
        List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
        foreach (var location in locations)
        {
            var assetInfo = package.ConvertLocationToAssetInfo(location, null);
            assetInfos.Add(assetInfo);
        }

        var options = new BundleDownloaderOptions(assetInfos.ToArray(), recursiveDownload, downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, string[] locations, int downloadingMaxNumber, int failedTryAgain)
    {
        return package.CreateBundleDownloader(locations, false, downloadingMaxNumber, failedTryAgain);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, AssetInfo assetInfo, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
    {
        AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
        var options = new BundleDownloaderOptions(assetInfos, recursiveDownload, downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain)
    {
        return package.CreateBundleDownloader(assetInfo, false, downloadingMaxNumber, failedTryAgain);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, AssetInfo[] assetInfos, bool recursiveDownload, int downloadingMaxNumber, int failedTryAgain)
    {
        var options = new BundleDownloaderOptions(assetInfos, recursiveDownload, downloadingMaxNumber, failedTryAgain);
        return package.CreateResourceDownloader(options);
    }
    public static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
    {
        return package.CreateBundleDownloader(assetInfos, false, downloadingMaxNumber, failedTryAgain);
    }
    #endregion

    #region 资源解压
    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceUnpackerOperation CreateResourceUnpacker(this ResourcePackage package, int unpackingMaxNumber, int failedTryAgain)
    {
        var options = new ResourceUnpackerOptions(unpackingMaxNumber, failedTryAgain);
        return package.CreateResourceUnpacker(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceUnpackerOperation CreateResourceUnpacker(this ResourcePackage package, string tag, int unpackingMaxNumber, int failedTryAgain)
    {
        string[] tags = new string[] { tag };
        var options = new ResourceUnpackerOptions(tags, unpackingMaxNumber, failedTryAgain);
        return package.CreateResourceUnpacker(options);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceUnpackerOperation CreateResourceUnpacker(this ResourcePackage package, string[] tags, int unpackingMaxNumber, int failedTryAgain)
    {
        var options = new ResourceUnpackerOptions(tags, unpackingMaxNumber, failedTryAgain);
        return package.CreateResourceUnpacker(options);
    }
    #endregion

    #region 资源导入
    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceImporterOperation CreateResourceImporter(this ResourcePackage package, string[] filePaths, int importerMaxNumber, int failedTryAgain)
    {
        ImportFileInfo[] fileInfos = new ImportFileInfo[filePaths.Length];
        for (int i = 0; i < filePaths.Length; i++)
        {
            ImportFileInfo fileInfo = new ImportFileInfo();
            fileInfo.FilePath = filePaths[i];
            fileInfos[i] = fileInfo;
        }

        return package.CreateResourceImporter(fileInfos, importerMaxNumber, failedTryAgain);
    }

    /// <summary>
    /// 兼容Yoo2版本
    /// </summary>
    public static ResourceImporterOperation CreateResourceImporter(this ResourcePackage package, ImportFileInfo[] fileInfos, int importerMaxNumber, int failedTryAgain)
    {
        ImportBundleInfo[] bundleInfos = new ImportBundleInfo[fileInfos.Length];
        for (int i = 0; i < fileInfos.Length; i++)
        {
            bundleInfos[i] = new ImportBundleInfo(
                filePath: fileInfos[i].FilePath,
                bundleName: fileInfos[i].BundleName,
                bundleGuid: fileInfos[i].BundleGuid);
        }

        var options = new BundleImporterOptions(bundleInfos, importerMaxNumber, failedTryAgain);
        return package.CreateResourceImporter(options);
    }
    #endregion
}

public static class CompatibleAssetHandle
{
    public static GameObject InstantiateSync(this AssetHandle handle, Transform parent)
    {
        var options = new InstantiateOptions(true, parent, false);
        return handle.InstantiateSync(options);
    }
    public static GameObject InstantiateSync(this AssetHandle handle, Transform parent, bool worldPositionStays)
    {
        var options = new InstantiateOptions(true, parent, worldPositionStays);
        return handle.InstantiateSync(options);
    }
    public static GameObject InstantiateSync(this AssetHandle handle, Vector3 position, Quaternion rotation)
    {
        var options = new InstantiateOptions(true, position, rotation);
        return handle.InstantiateSync(options);
    }
    public static GameObject InstantiateSync(this AssetHandle handle, Vector3 position, Quaternion rotation, Transform parent)
    {
        var options = new InstantiateOptions(true, parent, position, rotation);
        return handle.InstantiateSync(options);
    }

    public static InstantiateOperation InstantiateAsync(this AssetHandle handle, Transform parent, bool actived = true)
    {
        var options = new InstantiateOptions(actived, parent, false);
        return handle.InstantiateAsync(options);
    }
    public static InstantiateOperation InstantiateAsync(this AssetHandle handle, Transform parent, bool worldPositionStays, bool actived = true)
    {
        var options = new InstantiateOptions(actived, parent, worldPositionStays);
        return handle.InstantiateAsync(options);
    }
    public static InstantiateOperation InstantiateAsync(this AssetHandle handle, Vector3 position, Quaternion rotation, bool actived = true)
    {
        var options = new InstantiateOptions(actived, position, rotation);
        return handle.InstantiateAsync(options);
    }
    public static InstantiateOperation InstantiateAsync(this AssetHandle handle, Vector3 position, Quaternion rotation, Transform parent, bool actived = true)
    {
        var options = new InstantiateOptions(actived, parent, position, rotation);
        return handle.InstantiateAsync(options);
    }
}