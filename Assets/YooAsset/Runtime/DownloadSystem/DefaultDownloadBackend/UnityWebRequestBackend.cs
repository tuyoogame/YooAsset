using System;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 下载后台
    /// </summary>
    /// <remarks>
    /// 基于 Unity 内置 UnityWebRequest 实现的下载后台。
    /// 支持自定义 UnityWebRequest 创建方式，例如添加证书验证、代理设置等。
    /// </remarks>
    internal sealed class UnityWebRequestBackend : IDownloadBackend
    {
        private readonly UnityWebRequestCreator _webRequestCreator;

        /// <summary>
        /// 后端名称
        /// </summary>
        public string Name
        {
            get { return nameof(UnityWebRequestBackend); }
        }

        /// <summary>
        /// 创建 UnityWebRequest 下载后端（使用默认创建方式）
        /// </summary>
        public UnityWebRequestBackend() : this(null)
        {
        }

        /// <summary>
        /// 创建 UnityWebRequest 下载后端
        /// </summary>
        /// <param name="webRequestCreator">
        /// 自定义 UnityWebRequest 创建委托（可选）。
        /// 如果为 null，则使用默认的 UnityWebRequest 构造方式。
        /// </param>
        public UnityWebRequestBackend(UnityWebRequestCreator webRequestCreator)
        {
            _webRequestCreator = webRequestCreator;
        }

        /// <summary>
        /// 驱动更新
        /// </summary>
        /// <remarks>
        /// UnityWebRequest 由 Unity 引擎自动驱动，无需额外更新。
        /// </remarks>
        public void Update()
        {
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 无需释放资源
        }

        /// <summary>
        /// 创建 HEAD 请求
        /// </summary>
        public IDownloadHeadRequest CreateHeadRequest(DownloadDataRequestArgs args)
        {
            return new UnityWebRequestHeadDownloader(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建文件下载请求
        /// </summary>
        public IDownloadFileRequest CreateFileRequest(DownloadFileRequestArgs args)
        {
            return new UnityWebRequestFileDownloader(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建字节下载请求
        /// </summary>
        public IDownloadBytesRequest CreateBytesRequest(DownloadDataRequestArgs args)
        {
            return new UnityWebRequestBytesDownloader(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建文本下载请求
        /// </summary>
        public IDownloadTextRequest CreateTextRequest(DownloadDataRequestArgs args)
        {
            return new UnityWebRequestTextDownloader(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建 AssetBundle 下载请求
        /// </summary>
        public IDownloadAssetBundleRequest CreateAssetBundleRequest(DownloadAssetBundleRequestArgs args)
        {
            return new UnityWebRequestAssetBundleDownloader(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建模拟下载请求
        /// </summary>
        public IDownloadFileRequest CreateSimulateRequest(DownloadSimulateRequestArgs args)
        {
            return new VirtualFileDownloader(args);
        }
    }
}
