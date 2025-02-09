#if UNITY_WEBGL && DOUYINMINIGAME
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using TTSDK;
using System.Linq;
using System;

public static class TiktokFileSystemCreater
{
    public static FileSystemParameters CreateByteGameFileSystemParameters(IRemoteServices remoteServices, string packageRoot)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.RuntimeExtension";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter("REMOTE_SERVICES", remoteServices);
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

    private readonly HashSet<string> _recorders = new HashSet<string>();
    private readonly Dictionary<string, string> _cacheFilePaths = new Dictionary<string, string>(10000);
    private TTFileSystemManager _fileSystemMgr;
    private string _ttCacheRoot = string.Empty;
    private string _recordsFilePath = string.Empty;

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
            return _recorders.Count;
        }
    }

    #region 自定义参数
    /// <summary>
    /// 自定义参数：远程服务接口
    /// </summary>
    public IRemoteServices RemoteServices { private set; get; } = null;
    #endregion


    public TiktokFileSystem()
    {
    }
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new TTFSInitializeOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new TTFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new TTFSRequestPackageVersionOperation(this, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, string clearMode, object clearParam)
    {
        var operation = new FSClearCacheFilesCompleteOperation();
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
    {
        param.MainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        param.FallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        var operation = new TTFSDownloadFileOperation(this, bundle, param);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
        {
            var operation = new TTFSLoadBundleOperation(this, bundle);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        else
        {
            string error = $"{nameof(TiktokFileSystem)} not support load bundle type : {bundle.BundleType}";
            var operation = new FSLoadBundleCompleteOperation(error);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
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
        _ttCacheRoot = rootDirectory;
        _recordsFilePath = PathUtility.Combine(FileRoot, "__GAME_FILE_CACHE", "cache_records");

        if (string.IsNullOrEmpty(_ttCacheRoot))
        {
            throw new System.Exception("请配置抖音小游戏的缓存根目录！");
        }

        // 注意：CDN服务未启用的情况下，使用抖音WEB服务器
        if (RemoteServices == null)
        {
            string webRoot = PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _fileSystemMgr = TT.GetFileSystemManager();
        
        // 读取本地文件缓存记录
        if (CheckCacheFileExist(_recordsFilePath))
        {
            string recordText = _fileSystemMgr.ReadFileSync(_recordsFilePath, "utf8");
            if (string.IsNullOrEmpty(recordText) == false)
            {
                string[] records = recordText.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var record in records)
                {
                    _recorders.Add(record);
                }
            }
        }
        else
        {
            _fileSystemMgr.WriteFileSync(_recordsFilePath, string.Empty);
        }
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
        return _recorders.Contains(filePath);
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
        string filePath = GetCacheFileLoadPath(bundle);
        if (CheckCacheFileExist(filePath))
            return _fileSystemMgr.ReadFileSync(filePath);
        else
            return Array.Empty<byte>();
    }
    public virtual string ReadBundleFileText(PackageBundle bundle)
    {
        string filePath = GetCacheFileLoadPath(bundle);
        if (CheckCacheFileExist(filePath))
            return _fileSystemMgr.ReadFileSync(filePath, "utf8");
        else
            return string.Empty;
    }

    #region 内部方法
    public TTFileSystemManager GetFileSystemMgr()
    {
        return _fileSystemMgr;
    }
    public bool CheckCacheFileExist(string filePath)
    {
        return _fileSystemMgr.AccessSync(filePath);
    }
    private string GetCacheFileLoadPath(PackageBundle bundle)
    {
        if (_cacheFilePaths.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = _fileSystemMgr.GetLocalCachedPathForUrl(bundle.FileName);
            _cacheFilePaths.Add(bundle.BundleGUID, filePath);
        }
        return filePath;
    }
    #endregion

    #region 本地记录
    public List<string> GetAllRecords()
    {
        return _recorders.ToList();
    }
    public bool RecordBundleFile(string filePath)
    {
        if (_recorders.Contains(filePath))
        {
            YooLogger.Error($"{nameof(TiktokFileSystem)} has element : {filePath}");
            return false;
        }

        _recorders.Add(filePath);
        // 抖音没有AppendFileSync方法，所以这里直接写入，等后面增加这个方法再修改
        _fileSystemMgr.WriteFileSync(_recordsFilePath, string.Join(",", _recorders));
        // _fileSystemMgr.AppendFileSync(_recordsFilePath, filePath + ",");
        return true;
    }
    public void TryRecordBundle(PackageBundle bundle)
    {
        string filePath = GetCacheFileLoadPath(bundle);
        if (_recorders.Contains(filePath) == false)
        {
            _recorders.Add(filePath);
            // 抖音没有AppendFileSync方法，所以这里直接写入，等后面增加这个方法再修改
            _fileSystemMgr.WriteFileSync(_recordsFilePath, string.Join(",", _recorders));
            // _fileSystemMgr.AppendFileSync(_recordsFilePath, filePath + ",");
        }
    }
    public void ClearAllRecords()
    {
        _recorders.Clear();
        _fileSystemMgr.WriteFileSync(_recordsFilePath, string.Empty);
    }
    public void ClearRecord(string filePath)
    {
        if (_recorders.Contains(filePath))
        {
            _recorders.Remove(filePath);
        }
        //TODO: 这里没做记录移除，因为耗时，后续可以看情况添加
    }
    #endregion
}
#endif
