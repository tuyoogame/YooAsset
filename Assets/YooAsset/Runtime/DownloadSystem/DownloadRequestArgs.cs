using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 下载请求的公共参数
    /// </summary>
    internal readonly struct DownloadRequestArgs
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 响应的超时时间（单位：秒）
        /// </summary>
        /// <remarks>
        /// 当 Timeout 设置为 0 时，不应用超时。
        /// </remarks>
        public int Timeout { get; }

        /// <summary>
        /// 看门狗超时时间（单位：秒）
        /// </summary>
        /// <remarks>
        /// <para>用于监控下载任务的数据接收情况</para>
        /// <para>当设置值为 0 时，表示禁用看门狗监控。</para>
        /// </remarks>
        public int WatchdogTimeout { get; }

        /// <summary>
        /// 自定义请求头（可选）
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>
        /// 构造下载请求公共参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeout">响应超时时间（秒），0 表示不应用超时。</param>
        /// <param name="watchdogTimeout">看门狗超时时间（秒），0 表示禁用。</param>
        /// <param name="headers">自定义请求头（可选）</param>
        public DownloadRequestArgs(string url, int timeout, int watchdogTimeout,
            IReadOnlyDictionary<string, string> headers = null)
        {
            Url = url;
            Timeout = timeout;
            WatchdogTimeout = watchdogTimeout;
            Headers = headers;
        }
    }

    /// <summary>
    /// 文件下载请求参数
    /// </summary>
    /// <remarks>
    /// 支持断点续传和追加写入
    /// </remarks>
    internal readonly struct DownloadFileRequestArgs
    {
        /// <summary>
        /// 公共请求参数
        /// </summary>
        public DownloadRequestArgs RequestArgs { get; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string SavePath { get; }

        /// <summary>
        /// 是否追加写入文件
        /// </summary>
        /// <remarks>
        /// 配合 ResumeOffset 使用，用于断点续传场景。
        /// </remarks>
        public bool AppendToFile { get; }

        /// <summary>
        /// 中止请求时是否删除目标文件
        /// </summary>
        public bool RemoveFileOnAbort { get; }

        /// <summary>
        /// 断点续传的起始字节（小于等于 0 表示不启用）
        /// </summary>
        /// <remarks>
        /// 推荐由后端自动设置 Range 请求头："bytes={ResumeOffset}-"。
        /// </remarks>
        public long ResumeOffset { get; }

        /// <summary>
        /// 构造文件下载请求参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeout">响应超时时间（秒），0 表示不应用超时。</param>
        /// <param name="watchdogTimeout">看门狗超时时间（秒），0 表示禁用。</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="appendToFile">是否追加写入文件（用于断点续传）</param>
        /// <param name="removeFileOnAbort">中止时是否删除目标文件</param>
        /// <param name="resumeOffset">断点续传的起始字节位置</param>
        /// <param name="headers">自定义请求头（可选）</param>
        public DownloadFileRequestArgs(
            string url,
            int timeout,
            int watchdogTimeout,
            string savePath,
            bool appendToFile,
            bool removeFileOnAbort,
            long resumeOffset,
            IReadOnlyDictionary<string, string> headers = null)
        {
            RequestArgs = new DownloadRequestArgs(
                url: url,
                timeout: timeout,
                watchdogTimeout: watchdogTimeout,
                headers: headers);
            SavePath = savePath;
            AppendToFile = appendToFile;
            RemoveFileOnAbort = removeFileOnAbort;
            ResumeOffset = resumeOffset;
        }

        /// <summary>
        /// 构造文件下载请求参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeout">响应超时时间（秒），0 表示不应用超时。</param>
        /// <param name="watchdogTimeout">看门狗超时时间（秒），0 表示禁用。</param>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="headers">自定义请求头（可选）</param>
        public DownloadFileRequestArgs(
            string url,
            int timeout,
            int watchdogTimeout,
            string savePath,
            IReadOnlyDictionary<string, string> headers = null)
        {
            RequestArgs = new DownloadRequestArgs(
                url: url,
                timeout: timeout,
                watchdogTimeout: watchdogTimeout,
                headers: headers);
            SavePath = savePath;
            AppendToFile = false;
            RemoveFileOnAbort = true;
            ResumeOffset = 0;
        }
    }

    /// <summary>
    /// 数据下载请求参数（通用）
    /// </summary>
    /// <remarks>
    /// 用于字节数组或文本等内存下载场景
    /// </remarks>
    internal readonly struct DownloadDataRequestArgs
    {
        /// <summary>
        /// 公共请求参数
        /// </summary>
        public DownloadRequestArgs RequestArgs { get; }

        /// <summary>
        /// 构造数据下载请求参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeout">响应超时时间（秒），0 表示不应用超时。</param>
        /// <param name="watchdogTimeout">看门狗超时时间（秒），0 表示禁用。</param>
        /// <param name="headers">自定义请求头（可选）</param>
        public DownloadDataRequestArgs(string url, int timeout, int watchdogTimeout, IReadOnlyDictionary<string, string> headers = null)
        {
            RequestArgs = new DownloadRequestArgs(
                url: url,
                timeout: timeout,
                watchdogTimeout: watchdogTimeout,
                headers: headers);
        }
    }

    /// <summary>
    /// AssetBundle 下载请求参数
    /// </summary>
    /// <remarks>
    /// 支持 Unity 内置缓存机制和 CRC 校验
    /// </remarks>
    internal readonly struct DownloadAssetBundleRequestArgs
    {
        /// <summary>
        /// 公共请求参数
        /// </summary>
        public DownloadRequestArgs RequestArgs { get; }

        /// <summary>
        /// 禁用 Unity 的网络缓存
        /// </summary>
        public bool DisableUnityWebCache { get; }

        /// <summary>
        /// AssetBundle 文件哈希（用于 UnityWebRequest 的缓存）
        /// </summary>
        /// <remarks>
        /// 仅当 DisableUnityWebCache 为 false 时需要
        /// </remarks>
        public string FileHash { get; }

        /// <summary>
        /// Unity CRC 校验值
        /// </summary>
        public uint UnityCrc { get; }

        /// <summary>
        /// Web 平台策略
        /// </summary>
        public IWebPlatformStrategy PlatformStrategy { get; }

        /// <summary>
        /// 构造 AssetBundle 下载请求参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeout">响应超时时间（秒），0 表示不应用超时。</param>
        /// <param name="watchdogTimeout">看门狗超时时间（秒），0 表示禁用。</param>
        /// <param name="disableUnityWebCache">是否禁用 Unity 内置缓存</param>
        /// <param name="fileHash">文件哈希（启用缓存时必须提供）</param>
        /// <param name="unityCrc">Unity CRC 校验值</param>
        /// <param name="platformStrategy">WebGL 平台策略</param>
        /// <param name="headers">自定义请求头（可选）</param>
        public DownloadAssetBundleRequestArgs(
            string url,
            int timeout,
            int watchdogTimeout,
            bool disableUnityWebCache,
            string fileHash,
            uint unityCrc,
            IWebPlatformStrategy platformStrategy,
            IReadOnlyDictionary<string, string> headers = null)
        {
            RequestArgs = new DownloadRequestArgs(
                url: url,
                timeout: timeout,
                watchdogTimeout: watchdogTimeout,
                headers: headers);
            DisableUnityWebCache = disableUnityWebCache;
            FileHash = fileHash;
            UnityCrc = unityCrc;
            PlatformStrategy = platformStrategy;
        }
    }

    /// <summary>
    /// 模拟下载请求参数
    /// </summary>
    /// <remarks>
    /// 用于编辑器模式下模拟下载进度，不发起网络请求。
    /// </remarks>
    internal readonly struct SimulatedDownloadRequestArgs
    {
        /// <summary>
        /// 请求地址（仅用于标识）
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 模拟的文件大小（字节）
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// 模拟的下载速度（字节/秒）
        /// </summary>
        public long DownloadSpeed { get; }

        /// <summary>
        /// 构造模拟下载请求参数
        /// </summary>
        /// <param name="url">请求地址（仅用于标识）</param>
        /// <param name="fileSize">模拟的文件大小（字节）</param>
        /// <param name="downloadSpeed">模拟的下载速度（字节/秒）</param>
        public SimulatedDownloadRequestArgs(string url, long fileSize, long downloadSpeed)
        {
            Url = url;
            FileSize = Math.Max(fileSize, 0);
            DownloadSpeed = downloadSpeed;
        }
    }
}