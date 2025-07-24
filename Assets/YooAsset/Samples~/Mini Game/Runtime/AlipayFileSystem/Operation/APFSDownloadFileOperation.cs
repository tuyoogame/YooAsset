#if UNITY_WEBGL && UNITY_ALIMINIGAME
using UnityEngine;
using YooAsset;

internal class APFSDownloadFileOperation : FSDownloadFileOperation
{
    protected enum ESteps
    {
        None,
        CreateRequest,
        CheckRequest,
        TryAgain,
        Done,
    }

    private readonly AlipayFileSystem _fileSystem;
    private readonly DownloadFileOptions _options;
    private UnityWebCacheRequestOperation _webCacheRequestOp;
    private int _requestCount = 0;
    private float _tryAgainTimer;
    private int _failedTryAgain;
    private ESteps _steps = ESteps.None;

    internal APFSDownloadFileOperation(AlipayFileSystem fileSystem, PackageBundle bundle, DownloadFileOptions options) : base(bundle)
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

                //TODO 需要验证插件请求器的下载进度
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