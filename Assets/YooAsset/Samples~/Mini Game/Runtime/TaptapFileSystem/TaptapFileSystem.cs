#if UNITY_WEBGL && TAPMINIGAME
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using TapTapMiniGame;

public static class TaptapFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteServices remoteServices)
    {
        string fileSystemClass = $"{nameof(TaptapFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }

    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteServices remoteServices, IWebDecryptionServices decryptionServices)
    {
        string fileSystemClass = $"{nameof(TaptapFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
        return fileSystemParams;
    }
}

/// <summary>
/// TapTap小游戏
/// 参考：https://developer.taptap.cn/minigameapidoc/dev/engine/unity-adaptation/guide/
/// </summary>
internal class TaptapFileSystem : IFileSystem
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
    private TapFileSystemManager _fileSystemMgr;
    private string _tapCacheRoot = string.Empty;

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
            return _tapCacheRoot;
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


    public TaptapFileSystem()
    {
    }
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new TPFSInitializeOperation(this);
        return operation;
    }
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new TPFSLoadPackageManifestOperation(this, packageVersion, timeout);
        return operation;
    }
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new TPFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
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
        var operation = new TPFSDownloadFileOperation(this, bundle, options);
        return operation;
    }
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
        {
            var operation = new TPFSLoadBundleOperation(this, bundle);
            return operation;
        }
        else
        {
            string error = $"{nameof(TaptapFileSystem)} not support load bundle type : {bundle.BundleType}";
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
    public virtual void OnCreate(string packageName, string rootDirectory)
    {
        PackageName = packageName;
        _tapCacheRoot = rootDirectory;

        if (string.IsNullOrEmpty(_tapCacheRoot))
        {
            throw new System.Exception("请配置小游戏的缓存根目录！");
        }

        // 注意：CDN服务未启用的情况下，使用WEB服务器
        if (RemoteServices == null)
        {
            string webRoot = PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _fileSystemMgr = Tap.GetFileSystemManager();
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
    public TapFileSystemManager GetFileSystemMgr()
    {
        return _fileSystemMgr;
    }
    public bool CheckCacheFileExist(PackageBundle bundle)
    {
        //TODO : 效率极低
        /*
        string filePath = GetCacheFileLoadPath(bundle);
        string result = _fileSystemMgr.AccessSync(filePath);
        return result.Equals("access:ok", StringComparison.Ordinal);
        */
        return false;
    }
    private string GetCacheFileLoadPath(PackageBundle bundle)
    {
        if (_cacheFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = PathUtility.Combine(_tapCacheRoot, bundle.FileName);
            _cacheFilePathMapping.Add(bundle.BundleGUID, filePath);
        }
        return filePath;
    }
    #endregion
}
#endif