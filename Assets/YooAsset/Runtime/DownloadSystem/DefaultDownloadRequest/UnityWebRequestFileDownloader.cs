using System;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 文件下载器
    /// </summary>
    /// <remarks>
    /// 将下载内容保存到本地文件，支持断点续传和追加写入。
    /// </remarks>
    internal sealed class UnityWebRequestFileDownloader : UnityWebRequestDownloaderBase, IDownloadFileRequest
    {
        private readonly DownloadFileRequestArgs _args;

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string SavePath
        {
            get { return _args.SavePath; }
        }

        /// <summary>
        /// 构造文件下载器
        /// </summary>
        /// <param name="args">文件下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestFileDownloader(DownloadFileRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.URL, webRequestCreator)
        {
            _args = args;
        }

        /// <summary>
        /// 创建 UnityWebRequest
        /// </summary>
        protected override void CreateWebRequest()
        {
            var handler = new DownloadHandlerFile(_args.SavePath, _args.AppendToFile);
            handler.removeFileOnAbort = _args.RemoveFileOnAbort;

            _webRequest = CreateUnityWebRequestGet(URL);
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;

            // 断点续传：设置 Range 请求头
            if (_args.ResumeFromBytes > 0)
            {
                _webRequest.SetRequestHeader("Range", $"bytes={_args.ResumeFromBytes}-");
            }

            ApplyRequestOptions(_args.Timeout, _args.WatchdogTime, _args.Headers);
        }
    }
}
