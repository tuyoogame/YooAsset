using UnityEngine;

namespace YooAsset
{
    internal class LoadWebNormalAssetBundleOperation : LoadWebAssetBundleOperation
    {
        protected enum ESteps
        {
            None,
            CreateRequest,
            CheckRequest,
            TryAgain,
            Done,
        }

        private readonly PackageBundle _bundle;
        private readonly DownloadFileOptions _options;
        private readonly bool _disableUnityWebCache;
        private UnityAssetBundleRequestOperation _unityAssetBundleRequestOp;

        protected int _requestCount = 0;
        protected float _tryAgainTimer;
        protected int _failedTryAgain;
        private ESteps _steps = ESteps.None;


        internal LoadWebNormalAssetBundleOperation(PackageBundle bundle, DownloadFileOptions options, bool disableUnityWebCache)
        {
            _bundle = bundle;
            _options = options;
            _disableUnityWebCache = disableUnityWebCache;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                string url = GetRequestURL();
                _unityAssetBundleRequestOp = new UnityAssetBundleRequestOperation(_bundle, _disableUnityWebCache, url);
                _unityAssetBundleRequestOp.StartOperation();
                AddChildOperation(_unityAssetBundleRequestOp);
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                _unityAssetBundleRequestOp.UpdateOperation();
                Progress = _unityAssetBundleRequestOp.Progress;
                DownloadedBytes = _unityAssetBundleRequestOp.DownloadedBytes;
                DownloadProgress = _unityAssetBundleRequestOp.DownloadProgress;
                if (_unityAssetBundleRequestOp.IsDone == false)
                    return;

                if (_unityAssetBundleRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    Result = _unityAssetBundleRequestOp.Result;
                }
                else
                {
                    if (_failedTryAgain > 0)
                    {
                        _steps = ESteps.TryAgain;
                        YooLogger.Warning($"Failed download : {_unityAssetBundleRequestOp.URL} Try again !");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = _unityAssetBundleRequestOp.Error;
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
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            _requestCount++;
            if (_requestCount % 2 == 0)
                return _options.FallbackURL;
            else
                return _options.MainURL;
        }
    }
}