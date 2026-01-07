using System;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 文本下载器
    /// </summary>
    /// <remarks>
    /// 将下载内容解析为 UTF-8 文本字符串。
    /// </remarks>
    internal sealed class UnityWebRequestTextDownloader : UnityWebRequestDownloaderBase, IDownloadTextRequest
    {
        private readonly DownloadDataRequestArgs _args;

        /// <summary>
        /// 下载结果（文本字符串）
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// 构造文本下载器
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestTextDownloader(DownloadDataRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.URL, webRequestCreator)
        {
            _args = args;
        }

        /// <summary>
        /// 创建 UnityWebRequest
        /// </summary>
        protected override void CreateWebRequest()
        {
            var handler = new DownloadHandlerBuffer();
            _webRequest = CreateUnityWebRequestGet(URL);
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            ApplyRequestOptions(_args.Timeout, _args.WatchdogTime, _args.Headers);
        }

        /// <summary>
        /// 请求成功时的回调
        /// </summary>
        protected override void OnRequestSucceed()
        {
            Result = _webRequest.downloadHandler.text;
        }
    }
}
