#if UNITY_WEBGL && WEIXINMINIGAME
using UnityEngine;
using YooAsset;

internal class WXFSDownloadFileOperation : FSDownloadFileOperation
{
    protected enum ESteps
    {
        None,
        CreateRequest,
        CheckRequest,
        TryAgain,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly DownloadFileOptions _options;
    private UnityWebCacheRequestOperation _webCacheRequestOp;
    private int _requestCount = 0;
    private float _tryAgainTimer;
    private int _failedTryAgain;
    private ESteps _steps = ESteps.None;

    internal WXFSDownloadFileOperation(WechatFileSystem fileSystem, PackageBundle bundle, DownloadFileOptions options) : base(bundle)
    {
        _fileSystem = fileSystem;
        _options = options;
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
            string url = GetRequestURL();
            _webCacheRequestOp = new UnityWebCacheRequestOperation(url);
            _webCacheRequestOp.SetRequestHeader("wechatminigame-preload", "1");
            _webCacheRequestOp.StartOperation();
            AddChildOperation(_webCacheRequestOp);
            _steps = ESteps.CheckRequest;
        }

        // 检测下载结果
        if (_steps == ESteps.CheckRequest)
        {
            _webCacheRequestOp.UpdateOperation();
            Progress = _webCacheRequestOp.Progress;
            DownloadProgress = _webCacheRequestOp.DownloadProgress;
            DownloadedBytes = _webCacheRequestOp.DownloadedBytes;
            if (_webCacheRequestOp.IsDone == false)
                return;

            if (_webCacheRequestOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;

                //TODO 解决微信小游戏插件问题
                // Issue : https://github.com/wechat-miniprogram/minigame-unity-webgl-transform/issues/108#
                DownloadProgress = 1f;
                DownloadedBytes = Bundle.FileSize;
                Progress = 1f;
            }
            else
            {
                if (_failedTryAgain > 0)
                {
                    _steps = ESteps.TryAgain;
                    YooLogger.Warning($"Failed download : {_webCacheRequestOp.URL} Try again !");
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webCacheRequestOp.Error;
                    YooLogger.Error(Error);
                }
            }
        }

        // 重新尝试下载
        if (_steps == ESteps.TryAgain)
        {
            _tryAgainTimer += Time.unscaledDeltaTime;
            if (_tryAgainTimer > 1f)
            {
                _tryAgainTimer = 0f;
                _failedTryAgain--;
                Progress = 0f;
                DownloadProgress = 0f;
                DownloadedBytes = 0;
                _steps = ESteps.CreateRequest;
            }
        }
    }

    /// <summary>
    /// 获取网络请求地址
    /// </summary>
    private string GetRequestURL()
    {
        // 轮流返回请求地址
        _requestCount++;
        if (_requestCount % 2 == 0)
            return _options.FallbackURL;
        else
            return _options.MainURL;
    }
}
#endif