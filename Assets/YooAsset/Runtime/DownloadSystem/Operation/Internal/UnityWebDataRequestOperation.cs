using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal class UnityWebDataRequestOperation : UnityWebRequestOperation
    {
        private UnityWebRequestAsyncOperation _requestOperation;

        /// <summary>
        /// 请求结果
        /// </summary>
        public byte[] Result { private set; get; }


        internal UnityWebDataRequestOperation(string url, int timeout = 60) : base(url, timeout)
        {
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
                    Result = _webRequest.downloadHandler.data;
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
            DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _requestOperation = _webRequest.SendWebRequest();
        }
    }
}