#if UNITY_WEBGL && WEIXINMINIGAME
using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using WeChatWASM;

public static class WechatFileSystemCreater
{
    public static FileSystemParameters CreateWechatFileSystemParameters(IRemoteServices remoteServices)
    {
        string fileSystemClass = $"{nameof(WechatFileSystem)},YooAsset.RuntimeExtension";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
        fileSystemParams.AddParameter("REMOTE_SERVICES", remoteServices);
        return fileSystemParams;
    }
}

/// <summary>
/// 微信小游戏文件系统
/// 参考：https://wechat-miniprogram.github.io/minigame-unity-webgl-transform/Design/UsingAssetBundle.html
/// </summary>
internal class WechatFileSystem : IFileSystem
{
    private class WebRemoteServices : IRemoteServices
    {
        private readonly string _webPackageRoot;
        protected readonly Dictionary<string, string> _mapping = new Dictionary<string, string>(10000);

        public WebRemoteServices(string buildinPackRoot)
        {
            _webPackageRoot = buildinPackRoot;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return GetFileLoadURL(fileName);
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return GetFileLoadURL(fileName);
        }

        private string GetFileLoadURL(string fileName)
        {
            if (_mapping.TryGetValue(fileName, out string url) == false)
            {
                string filePath = PathUtility.Combine(_webPackageRoot, fileName);
                url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                _mapping.Add(fileName, url);
            }
            return url;
        }
    }

    /// <summary>
    /// Key:资源包GUID Value:缓存路径
    /// </summary>
    private readonly Dictionary<string, string> _cacheFilePaths = new Dictionary<string, string>(10000);
    private WXFileSystemManager _wxFileSystemMgr;
    private string _fileCacheRoot = string.Empty;

    /// <summary>
    /// 包裹名称
    /// </summary>
    public string PackageName { private set; get; }

    /// <summary>
    /// 文件根目录
    /// </summary>
    public string FileRoot
    {
        get
        {
            return _fileCacheRoot;
        }
    }

    /// <summary>
    /// 文件数量
    /// </summary>
    public int FileCount
    {
        get
        {
            return 0;
        }
    }

    #region 自定义参数
    /// <summary>
    /// 自定义参数：远程服务接口
    /// </summary>
    public IRemoteServices RemoteServices { private set; get; } = null;
    #endregion


    public WechatFileSystem()
    {
    }
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new WXFSInitializeOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new WXFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new WXFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync()
    {
        var operation = new WXFSClearAllBundleFilesOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest)
    {
        var operation = new WXFSClearUnusedBundleFilesAsync(this, manifest);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
    {
        param.MainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        param.FallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        var operation = new WXFSDownloadFileOperation(this, bundle, param);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        var operation = new WXFSLoadBundleOperation(this, bundle);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual void UnloadBundleFile(PackageBundle bundle, object result)
    {
        AssetBundle assetBundle = result as AssetBundle;
        if (assetBundle != null)
            assetBundle.WXUnload(true);
    }

    public virtual void SetParameter(string name, object value)
    {
        if (name == "REMOTE_SERVICES")
        {
            RemoteServices = (IRemoteServices)value;
        }
        else
        {
            YooLogger.Warning($"Invalid parameter : {name}");
        }
    }
    public virtual void OnCreate(string packageName, string rootDirectory)
    {
        PackageName = packageName;

        // 注意：CDN服务未启用的情况下，使用微信WEB服务器
        if (RemoteServices == null)
        {
            string webRoot = PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _wxFileSystemMgr = WX.GetFileSystemManager();
        _fileCacheRoot = WX.env.USER_DATA_PATH; //注意：如果有子目录，请修改此处！
        //_fileCacheRoot = PathUtility.Combine(WX.PluginCachePath, $"StreamingAssets/WebGL");
    }
    public virtual void OnUpdate()
    {
    }

    public virtual bool Belong(PackageBundle bundle)
    {
        return true;
    }
    public virtual bool Exists(PackageBundle bundle)
    {
        string filePath = GetCacheFileLoadPath(bundle);
        return CheckCacheFileExist(filePath);
    }
    public virtual bool NeedDownload(PackageBundle bundle)
    {
        if (Belong(bundle) == false)
            return false;

        return Exists(bundle) == false;
    }
    public virtual bool NeedUnpack(PackageBundle bundle)
    {
        return false;
    }
    public virtual bool NeedImport(PackageBundle bundle)
    {
        return false;
    }

    public virtual byte[] ReadFileData(PackageBundle bundle)
    {
        string filePath = GetCacheFileLoadPath(bundle);
        if (CheckCacheFileExist(filePath))
            return _wxFileSystemMgr.ReadFileSync(filePath);
        else
            return Array.Empty<byte>();
    }
    public virtual string ReadFileText(PackageBundle bundle)
    {
        string filePath = GetCacheFileLoadPath(bundle);
        if (CheckCacheFileExist(filePath))
            return _wxFileSystemMgr.ReadFileSync(filePath, "utf8");
        else
            return string.Empty;
    }

    #region 内部方法
    public bool CheckCacheFileExist(string filePath)
    {
        string result = _wxFileSystemMgr.AccessSync(filePath);
        return result.Equals("access:ok");
    }
    public string GetCacheFileLoadPath(PackageBundle bundle)
    {
        if (_cacheFilePaths.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = PathUtility.Combine(_fileCacheRoot, bundle.FileName);
            _cacheFilePaths.Add(bundle.BundleGUID, filePath);
        }
        return filePath;
    }
    #endregion
}
#endif