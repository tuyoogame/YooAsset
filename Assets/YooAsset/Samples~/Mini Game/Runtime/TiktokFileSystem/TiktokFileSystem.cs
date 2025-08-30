#if UNITY_WEBGL && DOUYINMINIGAME
using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using TTSDK;

public static class TiktokFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteServices remoteServices)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }

    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteServices remoteServices, IWebDecryptionServices decryptionServices)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
        return fileSystemParams;
    }
}

/// <summary>
/// 抖音小游戏文件系统
/// 参考：https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/know
/// </summary>
internal class TiktokFileSystem : IFileSystem
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

    private readonly Dictionary<string, string> _cacheFilePathMapping = new Dictionary<string, string>(10000);
    private TTFileSystemManager _fileSystemMgr;
    private string _ttCacheRoot = string.Empty;

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
            return _ttCacheRoot;
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

    /// <summary>
    ///  自定义参数：解密方法类
    /// </summary>
    public IWebDecryptionServices DecryptionServices { private set; get; }

    /// <summary>
    /// 自定义参数：资源清单服务类
    /// </summary>
    public IManifestRestoreServices ManifestServices { private set; get; }
    #endregion


    public TiktokFileSystem()
    {
    }
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new TTFSInitializeOperation(this);
        return operation;
    }
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new TTFSLoadPackageManifestOperation(this, packageVersion, timeout);
        return operation;
    }
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new TTFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
        return operation;
    }
    public virtual FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, ClearCacheFilesOptions options)
    {
        var operation = new FSClearCacheFilesCompleteOperation();
        return operation;
    }
    public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadFileOptions options)
    {
        string mainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        string fallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        options.SetURL(mainURL, fallbackURL);
        var operation = new TTFSDownloadFileOperation(this, bundle, options);
        return operation;
    }
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
        {
            var operation = new TTFSLoadBundleOperation(this, bundle);
            return operation;
        }
        else
        {
            string error = $"{nameof(TiktokFileSystem)} not support load bundle type : {bundle.BundleType}";
            var operation = new FSLoadBundleCompleteOperation(error);
            return operation;
        }
    }

    public virtual void SetParameter(string name, object value)
    {
        if (name == FileSystemParametersDefine.REMOTE_SERVICES)
        {
            RemoteServices = (IRemoteServices)value;
        }
        else if (name == FileSystemParametersDefine.DECRYPTION_SERVICES)
        {
            DecryptionServices = (IWebDecryptionServices)value;
        }
        else if (name == FileSystemParametersDefine.MANIFEST_SERVICES)
        {
            ManifestServices = (IManifestRestoreServices)value;
        }
        else
        {
            YooLogger.Warning($"Invalid parameter : {name}");
        }
    }
    public virtual void OnCreate(string packageName, string packageRoot)
    {
        PackageName = packageName;
        _ttCacheRoot = packageRoot;

        if (string.IsNullOrEmpty(_ttCacheRoot))
        {
            throw new System.Exception("请配置小游戏的缓存根目录！");
        }

        // 注意：CDN服务未启用的情况下，使用WEB服务器
        if (RemoteServices == null)
        {
            string webRoot = PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _fileSystemMgr = TT.GetFileSystemManager();
    }
    public virtual void OnDestroy()
    {
    }

    public virtual bool Belong(PackageBundle bundle)
    {
        return true;
    }
    public virtual bool Exists(PackageBundle bundle)
    {
        return CheckCacheFileExist(bundle);
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

    public virtual string GetBundleFilePath(PackageBundle bundle)
    {
        return GetCacheFileLoadPath(bundle);
    }
    public virtual byte[] ReadBundleFileData(PackageBundle bundle)
    {
        if (CheckCacheFileExist(bundle))
        {
            string filePath = GetCacheFileLoadPath(bundle);
            return _fileSystemMgr.ReadFileSync(filePath);
        }
        else
        {
            return Array.Empty<byte>();
        }
    }
    public virtual string ReadBundleFileText(PackageBundle bundle)
    {
        if (CheckCacheFileExist(bundle))
        {
            string filePath = GetCacheFileLoadPath(bundle);
            return _fileSystemMgr.ReadFileSync(filePath, "utf8");
        }
        else
        {
            return string.Empty;
        }
    }

    #region 内部方法
    public TTFileSystemManager GetFileSystemMgr()
    {
        return _fileSystemMgr;
    }
    public bool CheckCacheFileExist(PackageBundle bundle)
    {
        string url = RemoteServices.GetRemoteMainURL(bundle.FileName);
        return _fileSystemMgr.IsUrlCached(url);
    }
    private string GetCacheFileLoadPath(PackageBundle bundle)
    {
        if (_cacheFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = _fileSystemMgr.GetLocalCachedPathForUrl(bundle.FileName);
            _cacheFilePathMapping.Add(bundle.BundleGUID, filePath);
        }
        return filePath;
    }
    #endregion
}
#endif