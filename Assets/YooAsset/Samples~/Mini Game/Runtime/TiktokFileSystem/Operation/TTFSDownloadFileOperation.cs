#if UNITY_WEBGL && DOUYINMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

internal class TTFSDownloadFileOperation : DefaultDownloadFileOperation
{
    private TiktokFileSystem _fileSystem;
    private ESteps _steps = ESteps.None;

    internal TTFSDownloadFileOperation(TiktokFileSystem fileSystem, PackageBundle bundle, DownloadFileOptions options) : base(bundle, options)
    {
        _fileSystem = fileSystem;
    }
    internal override void InternalStart()
    {
        _steps = ESteps.CreateRequest;
    }
    internal override void InternalUpdate()
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
        //TODO : 抖音小游戏没有找到预下载方法
        _webRequest = UnityWebRequest.Get(_requestURL);
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
#endif