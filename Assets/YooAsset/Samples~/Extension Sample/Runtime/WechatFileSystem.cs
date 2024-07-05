#if UNITY_WECHAT_GAME
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using WeChatWASM;

public static class WechatFileSystemCreater
{
    public static FileSystemParameters CreateWechatFileSystemParameters(IRemoteServices remoteServices)
    {
        string fileSystemClass = $"{nameof(WechatFileSystem)},YooAsset.RuntimeExtension";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
        fileSystemParams.AddParameter("REMOTE_SERVICES", remoteServices);
        fileSystemParams.AddParameter("DISABLE_UNITY_WEB_CACHE", true);
        fileSystemParams.AddParameter("ALLOW_CROSS_ACCESS", true);
        return fileSystemParams;
    }
}

/// <summary>
/// 微信小游戏文件系统扩展
/// 参考：https://wechat-miniprogram.github.io/minigame-unity-webgl-transform/Design/UsingAssetBundle.html
/// </summary>
internal partial class WechatFileSystem : DefaultWebFileSystem
{
    private WXFileSystemManager _wxFileSystemMgr;
    private readonly Dictionary<string, string> _wxFilePaths = new Dictionary<string, string>(10000);
    private string _wxFileCacheRoot = string.Empty;

    public override void OnCreate(string packageName, string rootDirectory)
    {
        base.OnCreate(packageName, rootDirectory);

        _wxFileSystemMgr = WX.GetFileSystemManager();
        _wxFileCacheRoot = WX.env.USER_DATA_PATH; //注意：如果有子目录，请修改此处！
    }

    /// <summary>
    /// 重写资源文件下载方法
    /// </summary>
    public override FSDownloadFileOperation DownloadFileAsync(params object[] args)
    {
        PackageBundle bundle = args[0] as PackageBundle;
        int failedTryAgain = (int)args[2];
        int timeout = (int)args[3];

        string mainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        string fallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        var operation = new WechatDownloadFileOperation(this, bundle, mainURL, fallbackURL, failedTryAgain, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    /// <summary>
    /// 重写资源文件加载方法
    /// </summary>
    public override FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        var operation = new WechatLoadBundleOperation(this, bundle);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    /// <summary>
    /// 重写资源文件卸载方法
    /// </summary>
    public override void UnloadBundleFile(PackageBundle bundle, object result)
    {
        AssetBundle assetBundle = result as AssetBundle;
        if (assetBundle != null)
            assetBundle.WXUnload(true);
    }

    /// <summary>
    /// 重写查询方法
    /// </summary>
    public override bool Exists(PackageBundle bundle)
    {
        string filePath = GetWXFileLoadPath(bundle);
        string result = _wxFileSystemMgr.AccessSync(filePath);
        return result.Equals("access:ok");
    }

    #region 内部方法
    private string GetWXFileLoadPath(PackageBundle bundle)
    {
        if (_wxFilePaths.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = PathUtility.Combine(_wxFileCacheRoot, bundle.FileName);
            _wxFilePaths.Add(bundle.BundleGUID, filePath);
        }
        return filePath;
    }
    #endregion
}

internal partial class WechatFileSystem
{
    internal class WechatLoadBundleOperation : FSLoadBundleOperation
    {
        private enum ESteps
        {
            None,
            LoadBundleFile,
            Done,
        }

        private readonly WechatFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private UnityWebRequest _webRequest;
        private ESteps _steps = ESteps.None;

        internal WechatLoadBundleOperation(WechatFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadBundleFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBundleFile)
            {
                if (_webRequest == null)
                {
                    string mainURL = _fileSystem.RemoteServices.GetRemoteMainURL(_bundle.FileName);
                    _webRequest = WXAssetBundle.GetAssetBundle(mainURL);
                    _webRequest.SendWebRequest();
                }

                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = (long)_webRequest.downloadedBytes;
                Progress = DownloadProgress;
                if (_webRequest.isDone == false)
                    return;

                if (CheckRequestResult())
                {
                    _steps = ESteps.Done;
                    Result = (_webRequest.downloadHandler as DownloadHandlerWXAssetBundle).assetBundle;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            if (IsDone == false)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "WebGL platform not support sync load method !";
                UnityEngine.Debug.LogError(Error);
            }
        }
        public override void AbortDownloadOperation()
        {
        }

        private bool CheckRequestResult()
        {
#if UNITY_2020_3_OR_NEWER
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                Error = _webRequest.error;
                return false;
            }
            else
            {
                return true;
            }
#else
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
            {
                Error = _webRequest.error;
                return false;
            }
            else
            {
                return true;
            }
#endif
        }
    }
    internal class WechatDownloadFileOperation : DefaultDownloadFileOperation
    {
        private WechatFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;

        internal WechatDownloadFileOperation(WechatFileSystem fileSystem, PackageBundle bundle,
            string mainURL, string fallbackURL, int failedTryAgain, int timeout)
            : base(bundle, mainURL, fallbackURL, failedTryAgain, timeout)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalOnUpdate()
        {
            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                // 获取请求地址
                _requestURL = GetRequestURL();

                // 重置变量
                ResetRequestFiled();

                // 创建下载器
                CreateWebRequest();

                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = (long)_webRequest.downloadedBytes;
                Progress = DownloadProgress;
                if (_webRequest.isDone == false)
                {
                    CheckRequestTimeout();
                    return;
                }

                // 检查网络错误
                if (CheckRequestResult())
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.TryAgain;
                }

                // 注意：最终释放请求器
                DisposeWebRequest();
            }

            // 重新尝试下载
            if (_steps == ESteps.TryAgain)
            {
                if (FailedTryAgain <= 0)
                {
                    Status = EOperationStatus.Failed;
                    _steps = ESteps.Done;
                    YooLogger.Error(Error);
                    return;
                }

                _tryAgainTimer += Time.unscaledDeltaTime;
                if (_tryAgainTimer > 1f)
                {
                    FailedTryAgain--;
                    _steps = ESteps.CreateRequest;
                    YooLogger.Warning(Error);
                }
            }
        }

        private void CreateWebRequest()
        {
            _webRequest = WXAssetBundle.GetAssetBundle(_requestURL);
            _webRequest.SetRequestHeader("wechatminigame-preload", "1");
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _webRequest.SendWebRequest();
        }
        private void DisposeWebRequest()
        {
            if (_webRequest != null)
            {
                //注意：引擎底层会自动调用Abort方法
                _webRequest.Dispose();
                _webRequest = null;
            }
        }
    }
}
#endif