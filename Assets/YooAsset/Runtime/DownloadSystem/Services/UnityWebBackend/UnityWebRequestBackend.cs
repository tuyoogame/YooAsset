using System;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest 下载后台
    /// </summary>
    /// <remarks>
    /// <para>基于 Unity 内置 UnityWebRequest 实现的下载后台</para>
    /// <para>支持自定义 UnityWebRequest 创建方式，例如添加证书验证、代理设置等。</para>
    /// </remarks>
    internal sealed class UnityWebRequestBackend : IDownloadBackend
    {
        /// <summary>
        /// 自定义 UnityWebRequest 创建器（可为 null）
        /// </summary>
        private readonly UnityWebRequestCreator _webRequestCreator;

        /// <summary>
        /// 后台名称
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
        /// 自定义 UnityWebRequest 创建委托（可选）
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
        /// <param name="args">数据下载参数</param>
        /// <returns>HEAD 请求实例</returns>
        public IDownloadHeadRequest CreateHeadRequest(DownloadDataRequestArgs args)
        {
            return new UnityWebRequestHead(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建文件下载请求
        /// </summary>
        /// <param name="args">文件下载参数</param>
        /// <returns>文件下载请求实例</returns>
        public IDownloadFileRequest CreateFileRequest(DownloadFileRequestArgs args)
        {
            return new UnityWebRequestFile(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建字节下载请求
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <returns>字节下载请求实例</returns>
        public IDownloadBytesRequest CreateBytesRequest(DownloadDataRequestArgs args)
        {
            return new UnityWebRequestBytes(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建文本下载请求
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <returns>文本下载请求实例</returns>
        public IDownloadTextRequest CreateTextRequest(DownloadDataRequestArgs args)
        {
            return new UnityWebRequestText(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建 AssetBundle 下载请求
        /// </summary>
        /// <param name="args">AssetBundle 下载参数</param>
        /// <returns>AssetBundle 下载请求实例</returns>
        public IDownloadAssetBundleRequest CreateAssetBundleRequest(DownloadAssetBundleRequestArgs args)
        {
            return new UnityWebRequestAssetBundle(args, _webRequestCreator);
        }

        /// <summary>
        /// 创建模拟下载请求
        /// </summary>
        /// <param name="args">模拟下载参数</param>
        /// <returns>模拟下载请求实例</returns>
        public IDownloadFileRequest CreateSimulateRequest(SimulatedDownloadRequestArgs args)
        {
            return new SimulatedFileRequest(args);
        }
    }
}