#if UNITY_WEBGL && WEIXINMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using WeChatWASM;

internal class WXFSLoadBundleOperation : FSLoadBundleOperation
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

    internal WXFSLoadBundleOperation(WechatFileSystem fileSystem, PackageBundle bundle)
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
    internal override void InternalWaitForAsyncComplete()
    {
        if (_steps != ESteps.Done)
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
#endif