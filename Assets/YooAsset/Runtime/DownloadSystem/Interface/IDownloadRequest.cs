using System;

namespace YooAsset
{
    /// <summary>
    /// 可轮询的下载请求接口
    /// </summary>
    /// <remarks>
    /// 上层通常会在每帧轮询 Status IsDone，并在完成后读取结果。
    /// </remarks>
    internal interface IDownloadRequest : IDisposable
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        string URL { get; }

        /// <summary>
        /// 是否完成（成功/失败/中止）
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 请求状态
        /// </summary>
        EDownloadRequestStatus Status { get; }

        /// <summary>
        /// 当前下载进度（0f - 1f）
        /// </summary>
        /// <remarks>
        /// 部分情况下无法准确获取总长度，可返回 0。
        /// </remarks>
        float DownloadProgress { get; }

        /// <summary>
        /// 当前请求已接收的字节数
        /// </summary>
        /// <remarks>
        /// 断点续传场景下，该值仅表示"本次请求新增下载的字节数"，不包含已存在的本地文件长度。
        /// </remarks>
        long DownloadedBytes { get; }

        /// <summary>
        /// HTTP 返回码
        /// </summary>
        /// <remarks>
        /// 非 HTTP 协议可返回 0。使用 long 类型以兼容各种协议的返回码。
        /// </remarks>
        long HttpCode { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <remarks>
        /// 失败时不为空。
        /// </remarks>
        string Error { get; }

        /// <summary>
        /// 发起请求
        /// </summary>
        void SendRequest();

        /// <summary>
        /// 轮询请求
        /// </summary>
        void PollingRequest();

        /// <summary>
        /// 中止请求
        /// </summary>
        void AbortRequest();
    }

    /// <summary>
    /// HEAD 请求接口（仅获取响应头）
    /// </summary>
    /// <remarks>
    /// 用于检查资源是否存在、获取资源大小、检查缓存有效性等场景。
    /// 不下载实际内容，仅获取响应头信息。
    /// </remarks>
    internal interface IDownloadHeadRequest : IDownloadRequest
    {
        /// <summary>
        /// 获取 ETag 响应头
        /// </summary>
        /// <remarks>
        /// 用于缓存验证，如果服务器未返回则为 null。
        /// </remarks>
        string ETag { get; }

        /// <summary>
        /// 获取 Last-Modified 响应头
        /// </summary>
        /// <remarks>
        /// 资源最后修改时间，如果服务器未返回则为 null。
        /// </remarks>
        string LastModified { get; }

        /// <summary>
        /// 获取 Content-Type 响应头
        /// </summary>
        /// <remarks>
        /// 资源的 MIME 类型，如果服务器未返回则为 null。
        /// </remarks>
        string ContentType { get; }

        /// <summary>
        /// 预期下载的总字节数（Content-Length）
        /// </summary>
        /// <remarks>
        /// 从响应头 Content-Length 获取。
        /// 如果服务器未返回或请求未完成，返回 -1。
        /// 用于更准确的进度计算。
        /// </remarks>
        long ContentLength { get; }

        /// <summary>
        /// 获取响应头信息
        /// </summary>
        /// <param name="name">响应头名称（不区分大小写）</param>
        /// <returns>响应头的值，如果不存在或请求未完成则返回 null</returns>
        /// <remarks>
        /// 常用响应头：Content-Length、Content-Type、ETag、Last-Modified 等。
        /// </remarks>
        string GetResponseHeader(string name);
    }

    /// <summary>
    /// 文件下载请求接口
    /// </summary>
    /// <remarks>
    /// 将下载内容保存到指定的本地文件路径。
    /// </remarks>
    internal interface IDownloadFileRequest : IDownloadRequest
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        string SavePath { get; }
    }

    /// <summary>
    /// 内存下载请求接口（字节数组）
    /// </summary>
    /// <remarks>
    /// 将下载内容保存到内存中的字节数组。
    /// </remarks>
    internal interface IDownloadBytesRequest : IDownloadRequest
    {
        /// <summary>
        /// 下载结果（字节数组）
        /// </summary>
        /// <remarks>
        /// 仅在请求成功时可用，失败时为 null。
        /// </remarks>
        byte[] Result { get; }
    }

    /// <summary>
    /// 文本下载请求接口
    /// </summary>
    /// <remarks>
    /// 将下载内容解析为 UTF-8 文本字符串。
    /// </remarks>
    internal interface IDownloadTextRequest : IDownloadRequest
    {
        /// <summary>
        /// 下载结果（文本字符串）
        /// </summary>
        /// <remarks>
        /// 仅在请求成功时可用，失败时为 null。
        /// </remarks>
        string Result { get; }
    }

    /// <summary>
    /// AssetBundle 下载请求接口
    /// </summary>
    /// <remarks>
    /// 下载并加载 Unity AssetBundle 资源包。
    /// </remarks>
    internal interface IDownloadAssetBundleRequest : IDownloadRequest
    {
        /// <summary>
        /// 下载结果（AssetBundle 对象）
        /// </summary>
        /// <remarks>
        /// 仅在请求成功时可用，失败时为 null。
        /// </remarks>
        UnityEngine.AssetBundle Result { get; }
    }
}
