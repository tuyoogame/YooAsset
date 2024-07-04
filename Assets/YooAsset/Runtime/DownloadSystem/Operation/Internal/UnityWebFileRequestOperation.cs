using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal class UnityWebFileRequestOperation : UnityWebRequestOperation
    {
        private UnityWebRequestAsyncOperation _requestOperation;
        private readonly string _fileSavePath;

        internal UnityWebFileRequestOperation(string url, string fileSavePath, int timeout = 60) : base(url, timeout)
        {
            _fileSavePath = fileSavePath;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CreateRequest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CreateRequest)
            {
                _latestDownloadBytes = 0;
                _latestDownloadRealtime = Time.realtimeSinceStartup;

                CreateWebRequest();
                _steps = ESteps.Download;
            }

            if (_steps == ESteps.Download)
            {
                Progress = _requestOperation.progress;
                if (_requestOperation.isDone == false)
                {
                    CheckRequestTimeout();
                    return;
                }

                if (CheckRequestResult())
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
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
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            DisposeRequest();
        }

        private void CreateWebRequest()
        {
            _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
            DownloadHandlerFile handler = new DownloadHandlerFile(_fileSavePath);
            handler.removeFileOnAbort = true;
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _requestOperation = _webRequest.SendWebRequest();
        }
    }
}