using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal class DownloadHandlerAssetBundleOperation : DefaultDownloadFileOperation
    {
        private readonly DefaultWebFileSystem _fileSystem;
        private DownloadHandlerAssetBundle _downloadhandler;
        private ESteps _steps = ESteps.None;

        public AssetBundle Result { private set; get; }


        internal DownloadHandlerAssetBundleOperation(DefaultWebFileSystem fileSystem, PackageBundle bundle, DownloadParam param) : base(bundle, param)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

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
                    Result = _downloadhandler.assetBundle;
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
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            DisposeWebRequest();
        }

        private void CreateWebRequest()
        {
            _downloadhandler = CreateDownloadHandler();
            _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
            _webRequest.downloadHandler = _downloadhandler;
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
        private DownloadHandlerAssetBundle CreateDownloadHandler()
        {
            if (_fileSystem.DisableUnityWebCache)
            {
                var downloadhandler = new DownloadHandlerAssetBundle(_requestURL, 0);
#if UNITY_2020_3_OR_NEWER
                downloadhandler.autoLoadAssetBundle = false;
#endif
                return downloadhandler;
            }
            else
            {
                // 注意：优先从浏览器缓存里获取文件
                // The file hash defining the version of the asset bundle.
                uint unityCRC = Bundle.UnityCRC;
                Hash128 fileHash = Hash128.Parse(Bundle.FileHash);
                var downloadhandler = new DownloadHandlerAssetBundle(_requestURL, fileHash, unityCRC);
#if UNITY_2020_3_OR_NEWER
                downloadhandler.autoLoadAssetBundle = false;
#endif
                return downloadhandler;
            }
        }
    }
}