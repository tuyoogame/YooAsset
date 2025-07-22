using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal class UnityWebCacheRequestOperation : UnityWebRequestOperation
    {
        protected enum ESteps
        {
            None,
            CreateRequest,
            Download,
            Done,
        }

        private UnityWebRequestAsyncOperation _requestOperation;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private ESteps _steps = ESteps.None;


        internal UnityWebCacheRequestOperation(string url) : base(url)
        {
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

        /// <summary>
        /// 设置请求头信息
        /// </summary>
        public void SetRequestHeader(string name, string value)
        {
            _headers.Add(name, value);
        }

        private void CreateWebRequest()
        {
            _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
            _webRequest.disposeDownloadHandlerOnDispose = true;

            // 设置消息头
            foreach (var keyValuePair in _headers)
            {
                string name = keyValuePair.Key;
                string value = keyValuePair.Value;
                _webRequest.SetRequestHeader(name, value);
            }

            _requestOperation = _webRequest.SendWebRequest();
        }
    }
}