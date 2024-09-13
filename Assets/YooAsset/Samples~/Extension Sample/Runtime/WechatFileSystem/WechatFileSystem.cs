#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using WeChatWASM;

public static class WechatFileSystemCreater
{
    public static FileSystemParameters CreateWechatFileSystemParameters(IRemoteServices remoteServices,string rootDirectory = null)
    {
        string fileSystemClass = $"{nameof(WechatFileSystem)},YooAsset.RuntimeExtension";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, rootDirectory);
        fileSystemParams.AddParameter("REMOTE_SERVICES", remoteServices);
        return fileSystemParams;
    }

    /// <summary>
    /// AppVersion
    /// </summary>
    public static string AppVersion { get; set; }
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

    private readonly Dictionary<string, string> _cacheFilePaths = new Dictionary<string, string>(10000);
    private WXFileSystemManager _fileSystemManager;
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

        _fileSystemManager = WX.GetFileSystemManager();
        _fileCacheRoot = rootDirectory;// WX.PluginCachePath; //WX.env.USER_DATA_PATH; //注意：如果有子目录，请修改此处！
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
        string filePath = GetWXFileLoadPath(bundle);
        //Debug.Log($"CacheFile:{WX.GetCachePath($"StreamingAssets/WebGL/v1.0.0/{bundle.FileName}")}");
        return CheckWXFileIsExist(filePath);
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
        throw new System.NotImplementedException();
    }
    public virtual string ReadFileText(PackageBundle bundle)
    {
        throw new System.NotImplementedException();
    }
    public byte[] ReadFileData(string filePath)
    {
        if (CheckWXFileIsExist(filePath))
            return _wxFileSystemMgr.ReadFileSync(filePath);
        else
            return Array.Empty<byte>();
        //throw new System.NotImplementedException();
    }

    public string ReadFileText(string filePath)
    {
        if(CheckWXFileIsExist(filePath))
            return _wxFileSystemMgr.ReadFileSync(filePath, "utf8");
        else
            return string.Empty;
        //throw new System.NotImplementedException();
    }
    /// <summary>
    /// 获取所有缓存文件的路径
    /// </summary>
    /// <returns></returns>
    public Dictionary<string,string> GetWXAllCacheFilePath() 
    {
        return _wxFilePaths;
    }
    /// <summary>
    /// 判断微信缓存文件是否存在
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool CheckWXFileIsExist(string filePath)
    {
        string result = _wxFileSystemMgr.AccessSync(filePath);
        return result.Equals("access:ok");
    }
#region 调用微信小游戏接口删除缓存文件目录下所有文件
    public void ClearAllCacheFile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL && WEIXINMINIGAME
        ShowModalOption showModalOp = new ShowModalOption();
        showModalOp.title = "提示";
        showModalOp.content = "是否确定要清理缓存并重启";
        showModalOp.confirmText = "确定";
        showModalOp.cancelText = "取消";
        showModalOp.complete = (GeneralCallbackResult callResult) => { Debug.Log($"complete==={callResult.errMsg}"); };
        showModalOp.fail = (GeneralCallbackResult callResult) => { Debug.Log($"fail==={callResult.errMsg}"); };
        showModalOp.success = (ShowModalSuccessCallbackResult callResult) =>
        { 
            if(callResult.confirm)
                RestartMiniGame(); 
        };
        WX.ShowModal(showModalOp);
#endif
    }

    /// <summary>
    /// 微信小游戏清除缓存并且重启小游戏
    /// 参考小游戏=>出发吧麦芬
    /// </summary>
    private void RestartMiniGame()
    {
        WX.CleanAllFileCache((bool isOk) =>
        {
            RestartMiniProgramOption restartMini = new RestartMiniProgramOption();
            restartMini.complete = RestartMiniComplete;
            restartMini.fail = RestartMiniFailComplete;
            restartMini.success = RestartMiniSuccComplete;
            WX.RestartMiniProgram(restartMini);
        });
    }

    private void RestartMiniComplete(GeneralCallbackResult result)
    {
        Debug.Log($"RestartMiniComplete:{result.errMsg}");
    }

    private void RestartMiniFailComplete(GeneralCallbackResult result)
    {
        Debug.Log($"RestartMiniFailComplete:{result.errMsg}");
    }

    private void RestartMiniSuccComplete(GeneralCallbackResult result)
    {
        Debug.Log($"RestartMiniSuccComplete:{result.errMsg}");
    }

#endregion
#region 内部方法
    private string GetCacheFileLoadPath(PackageBundle bundle)
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