using System;

namespace YooAsset
{
    /// <summary>
    /// 下载请求接口
    /// </summary>
    internal interface IDownloadRequest : IDisposable
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        string Url { get; }

        /// <summary>
        /// 是否完成（成功/失败/中止）
        /// </summary>
        // TODO: 该属性具有轮询驱动语义，每次访问会推进内部状态机。后续优化为无副作用的纯查询属性。
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
        /// 非 HTTP 协议可返回 0
        /// </remarks>
        long HttpCode { get; }

        /// <summary>
        /// HTTP 错误信息
        /// </summary>
        string HttpError { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        string Error { get; }

        /// <summary>
        /// 发起请求
        /// </summary>
        void SendRequest();

        /// <summary>
        /// 中止请求
        /// </summary>
        void AbortRequest();
    }

    /// <summary>
    /// HEAD 请求接口（仅获取响应头）
    /// </summary>
    internal interface IDownloadHeadRequest : IDownloadRequest
    {
        /// <summary>
        /// ETag 响应头
        /// </summary>
        string ETag { get; }

        /// <summary>
        /// Last-Modified 响应头
        /// </summary>
        string LastModified { get; }

        /// <summary>
        /// Content-Type 响应头
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// 预期下载的总字节数
        /// </summary>
        /// <remarks>
        /// 服务器未返回或请求未完成时返回 -1
        /// </remarks>
        long ContentLength { get; }

        /// <summary>
        /// 获取响应头信息
        /// </summary>
        /// <param name="name">响应头名称（不区分大小写）</param>
        /// <returns>响应头的值，如果不存在或请求未完成则返回 null。</returns>
        string GetResponseHeader(string name);
    }

    /// <summary>
    /// 文件下载请求接口
    /// </summary>
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
