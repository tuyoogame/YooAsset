using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal class AssetBundleWebRequest : IWebRequester
    {
        private UnityWebRequest _webRequest;
        private DownloadHandlerAssetBundle _downloadhandler;
        private AssetBundle _cacheAssetBundle;
        private bool _getAssetBundle = false;

        public ERequestStatus Status { private set; get; } = ERequestStatus.None;
        public float DownloadProgress { private set; get; }
        public ulong DownloadedBytes { private set; get; }
        public string RequestNetError { private set; get; }
        public long RequestHttpCode { private set; get; }

        public AssetBundleWebRequest() { }
        public void Create(string requestURL, BundleInfo bundleInfo, params object[] args)
        {
            if (Status != ERequestStatus.None)
                throw new System.Exception("Should never get here !");

            if (args.Length == 0)
                throw new System.Exception("Not found param value");

            // 解析附加参数
            _getAssetBundle = (bool)args[0];

#if UNITY_WECHAT_GAME && !UNITY_EDITOR
            uint crc = bundleInfo.Bundle.UnityCRC;
            _webRequest = WeChatWASM.WXAssetBundle.GetAssetBundle(requestURL, crc);
            _webRequest.disposeDownloadHandlerOnDispose = false;
#else

            // 创建下载器
            _webRequest = DownloadHelper.NewRequest(requestURL);
            if (CacheHelper.DisableUnityCacheOnWebGL)
            {
                uint crc = bundleInfo.Bundle.UnityCRC;
                _downloadhandler = new DownloadHandlerAssetBundle(requestURL, crc);
            }
            else
            {
                uint crc = bundleInfo.Bundle.UnityCRC;
                var hash = Hash128.Parse(bundleInfo.Bundle.FileHash);
                _downloadhandler = new DownloadHandlerAssetBundle(requestURL, hash, crc);
            }
#if UNITY_2020_3_OR_NEWER
            _downloadhandler.autoLoadAssetBundle = false;
#endif
            _webRequest.downloadHandler = _downloadhandler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
#endif

            _webRequest.SendWebRequest();
            Status = ERequestStatus.InProgress;
        }
        public void Update()
        {
            if (Status == ERequestStatus.None)
                return;
            if (IsDone())
                return;

            DownloadProgress = _webRequest.downloadProgress;
            DownloadedBytes = _webRequest.downloadedBytes;
            if (_webRequest.isDone == false)
                return;

            // 检查网络错误
#if UNITY_2020_3_OR_NEWER
            RequestHttpCode = _webRequest.responseCode;
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                RequestNetError = _webRequest.error;
                Status = ERequestStatus.Error;
            }
            else
            {
                Status = ERequestStatus.Success;
            }
#else
            RequestHttpCode = _webRequest.responseCode;
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
            {
                RequestNetError = _webRequest.error;
                Status = ERequestStatus.Error;
            }
            else
            {
                Status = ERequestStatus.Success;
            }
#endif

            // 缓存加载的AssetBundle对象
            if (Status == ERequestStatus.Success)
            {
                if (_getAssetBundle)
                {
#if UNITY_WECHAT_GAME && !UNITY_EDITOR
                    _cacheAssetBundle = (_webRequest.downloadHandler as WeChatWASM.DownloadHandlerWXAssetBundle).assetBundle;
#else
                    _cacheAssetBundle = _downloadhandler.assetBundle;
#endif
                    if (_cacheAssetBundle == null)
                    {
                        RequestNetError = "assetBundle is null";
                        Status = ERequestStatus.Error;
                    }
                }
            }

            // 最终释放下载器
            DisposeWebRequest();
        }
        public void Abort()
        {
            // 如果下载任务还未开始
            if (Status == ERequestStatus.None)
            {
                RequestNetError = "user cancel";
                Status = ERequestStatus.Error;
            }
            else
            {
                // 注意：为了防止同一个文件强制停止之后立马创建新的请求，应该让进行中的请求自然终止。
                if (_webRequest != null)
                {
                    if (_webRequest.isDone == false)
                        _webRequest.Abort(); // If in progress, halts the UnityWebRequest as soon as possible.
                }
            }
        }
        public bool IsDone()
        {
            if (Status == ERequestStatus.Success || Status == ERequestStatus.Error)
                return true;
            else
                return false;
        }
        public object GetRequestObject()
        {
            return _cacheAssetBundle;
        }
        private void DisposeWebRequest()
        {
            if (_webRequest != null)
            {
                _webRequest.Dispose();
                _webRequest = null;
            }
        }
    }
}