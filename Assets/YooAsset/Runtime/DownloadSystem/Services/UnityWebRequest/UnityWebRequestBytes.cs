using System;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 字节下载器
    /// </summary>
    /// <remarks>
    /// 将下载内容保存到内存中的字节数组
    /// </remarks>
    internal sealed class UnityWebRequestBytes : UnityWebRequestBase, IDownloadBytesRequest
    {
        /// <summary>
        /// 下载结果（字节数组）
        /// </summary>
        public byte[] Result { get; private set; }

        /// <summary>
        /// 构造字节数组下载器
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestBytes(DownloadDataRequestArgs args, UnityWebRequestCreator webRequestCreator)
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
            Result = webRequest.downloadHandler.data;
        }
    }
}
