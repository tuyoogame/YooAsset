using System;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 文本下载器
    /// </summary>
    /// <remarks>
    /// 将下载内容解析为 UTF-8 文本字符串
    /// </remarks>
    internal sealed class UnityWebRequestText : UnityWebRequestBase, IDownloadTextRequest
    {
        /// <summary>
        /// 下载结果（文本字符串）
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// 构造文本下载器
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestText(DownloadDataRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.RequestArgs, webRequestCreator)
        {
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            var request = CreateGetWebRequest(Url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            return request;
        }

        protected override void OnRequestSucceeded(UnityWebRequest webRequest)
        {
            Result = webRequest.downloadHandler.text;
        }
    }
}
