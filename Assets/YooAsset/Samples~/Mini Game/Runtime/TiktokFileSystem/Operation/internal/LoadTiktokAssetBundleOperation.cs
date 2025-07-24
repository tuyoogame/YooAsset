#if UNITY_WEBGL && DOUYINMINIGAME
using UnityEngine;

namespace YooAsset
{
    internal class LoadTiktokAssetBundleOperation : LoadWebAssetBundleOperation
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
        private UnityTiktokAssetBundleRequestOperation _unityAssetBundleRequestOp;

        private int _requestCount = 0;
        private float _tryAgainTimer;
        private int _failedTryAgain;
        private ESteps _steps = ESteps.None;


        internal LoadTiktokAssetBundleOperation(PackageBundle bundle, DownloadFileOptions options)
        {
            _bundle = bundle;
            _options = options;
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
                _unityAssetBundleRequestOp = new UnityTiktokAssetBundleRequestOperation(_bundle, url);
                _unityAssetBundleRequestOp.StartOperation();
                AddChildOperation(_unityAssetBundleRequestOp);
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                _unityAssetBundleRequestOp.UpdateOperation();
                Progress = _unityAssetBundleRequestOp.Progress;
                DownloadProgress = _unityAssetBundleRequestOp.DownloadProgress;
                DownloadedBytes = _unityAssetBundleRequestOp.DownloadedBytes;
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
}
#endif