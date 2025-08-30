using UnityEngine;

namespace YooAsset
{
    internal class LoadWebEncryptAssetBundleOperation : LoadWebAssetBundleOperation
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
        private readonly IWebDecryptionServices _decryptionServices;
        private UnityWebDataRequestOperation _unityWebDataRequestOp;

        protected int _requestCount = 0;
        protected float _tryAgainTimer;
        protected int _failedTryAgain;
        private ESteps _steps = ESteps.None;

        internal LoadWebEncryptAssetBundleOperation(PackageBundle bundle, DownloadFileOptions options, IWebDecryptionServices decryptionServices)
        {
            _bundle = bundle;
            _options = options;
            _decryptionServices = decryptionServices;
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
                if (_decryptionServices == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"The {nameof(IWebDecryptionServices)} is null !";
                    YooLogger.Error(Error);
                    return;
                }

                string url = GetRequestURL();
                _unityWebDataRequestOp = new UnityWebDataRequestOperation(url, 0);
                _unityWebDataRequestOp.StartOperation();
                AddChildOperation(_unityWebDataRequestOp);
                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                _unityWebDataRequestOp.UpdateOperation();
                Progress = _unityWebDataRequestOp.Progress;
                DownloadProgress = _unityWebDataRequestOp.DownloadProgress;
                DownloadedBytes = _unityWebDataRequestOp.DownloadedBytes;
                if (_unityWebDataRequestOp.IsDone == false)
                    return;

                // 检查网络错误
                if (_unityWebDataRequestOp.Status == EOperationStatus.Succeed)
                {
                    AssetBundle assetBundle = LoadEncryptedAssetBundle(_unityWebDataRequestOp.Result);
                    if (assetBundle == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Failed load encrypted AssetBundle !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        Result = assetBundle;
                    }
                }
                else
                {
                    if (_failedTryAgain > 0)
                    {
                        _steps = ESteps.TryAgain;
                        YooLogger.Warning($"Failed download : {_unityWebDataRequestOp.URL} Try again !");
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = _unityWebDataRequestOp.Error;
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
        /// 加载加密资源文件
        /// </summary>
        private AssetBundle LoadEncryptedAssetBundle(byte[] fileData)
        {
            var fileInfo = new WebDecryptFileInfo();
            fileInfo.BundleName = _bundle.BundleName;
            fileInfo.FileLoadCRC = _bundle.UnityCRC;
            fileInfo.FileData = fileData;
            var decryptResult = _decryptionServices.LoadAssetBundle(fileInfo);
            return decryptResult.Result;
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