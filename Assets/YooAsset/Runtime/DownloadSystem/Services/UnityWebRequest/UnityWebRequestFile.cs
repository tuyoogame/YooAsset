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
    internal sealed class UnityWebRequestFile : UnityWebRequestBase, IDownloadFileRequest
    {
        /// <summary>
        /// 文件下载参数
        /// </summary>
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
        public UnityWebRequestFile(DownloadFileRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.RequestArgs, webRequestCreator)
        {
            _args = args;
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            var handler = new DownloadHandlerFile(_args.SavePath, _args.AppendToFile);
            handler.removeFileOnAbort = _args.RemoveFileOnAbort;

            var request = CreateGetWebRequest(Url);
            request.downloadHandler = handler;
            request.disposeDownloadHandlerOnDispose = true;

            // 断点续传：设置 Range 请求头
            if (_args.ResumeOffset > 0)
            {
                request.SetRequestHeader("Range", $"bytes={_args.ResumeOffset}-");
            }

            return request;
        }
    }
}
