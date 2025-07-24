#if UNITY_WEBGL && UNITY_ALIMINIGAME
using UnityEngine.Networking;
using UnityEngine;
using AlipaySdk;

namespace YooAsset
{
    internal class UnityAlipayAssetBundleRequestOperation : UnityWebRequestOperation
    {
        protected enum ESteps
        {
            None,
            CreateRequest,
            Download,
            Done,
        }

        private readonly PackageBundle _packageBundle;
        private UnityWebRequestAsyncOperation _requestOperation;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 请求结果
        /// </summary>
        public AssetBundle Result { private set; get; }

        internal UnityAlipayAssetBundleRequestOperation(PackageBundle bundle, string url) : base(url)
        {
            _packageBundle = bundle;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateRequest)
            {
                CreateWebRequest();
                _steps = ESteps.Download;
            }

            if (_steps == ESteps.Download)
            {
                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = (long)_webRequest.downloadedBytes;
                Progress = _requestOperation.progress;
                if (_requestOperation.isDone == false)
                    return;

                if (CheckRequestResult())
                {
                    var downloadHanlder = (DownloadHandlerAPAssetBundle)_webRequest.downloadHandler;
                    AssetBundle assetBundle = downloadHanlder.assetBundle;
                    if (assetBundle == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"URL : {_requestURL} Download handler asset bundle object is null !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Result = assetBundle;
                        Status = EOperationStatus.Succeed;

                        //TODO 需要验证插件请求器的下载进度
                        DownloadProgress = 1f;
                        DownloadedBytes = _packageBundle.FileSize;
                        Progress = 1f;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                }

                // 注意：最终释放请求器
                DisposeRequest();
            }
        }

        private void CreateWebRequest()
        {
            _webRequest = APAssetBundle.GetAssetBundle(_requestURL);
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _requestOperation = _webRequest.SendWebRequest();
        }
    }
}
#endif