using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest HEAD 请求下载器
    /// </summary>
    /// <remarks>
    /// <para>仅获取响应头信息，不下载实际内容。</para>
    /// <para>用于检查资源是否存在、获取资源大小、检查缓存有效性等场景</para>
    /// </remarks>
    internal sealed class UnityWebRequestHead : UnityWebRequestBase, IDownloadHeadRequest
    {
        /// <summary>
        /// 缓存的响应头（请求完成后从 WebRequest 复制）
        /// </summary>
        /// <remarks>
        /// WebRequest 释放后无法获取响应头，因此需要提前缓存。
        /// </remarks>
        private Dictionary<string, string> _cachedResponseHeaders;

        /// <summary>
        /// ETag 响应头
        /// </summary>
        public string ETag
        {
            get { return GetResponseHeader("ETag"); }
        }

        /// <summary>
        /// Last-Modified 响应头
        /// </summary>
        public string LastModified
        {
            get { return GetResponseHeader("Last-Modified"); }
        }

        /// <summary>
        /// Content-Type 响应头
        /// </summary>
        public string ContentType
        {
            get { return GetResponseHeader("Content-Type"); }
        }

        /// <summary>
        /// 预期下载的总字节数
        /// </summary>
        public long ContentLength
        {
            get
            {
                string contentLengthStr = GetResponseHeader("Content-Length");
                if (string.IsNullOrEmpty(contentLengthStr))
                    return -1;

                if (long.TryParse(contentLengthStr, out long contentLength))
                {
                    return contentLength;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// 构造 HEAD 请求下载器
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestHead(DownloadDataRequestArgs args, UnityWebRequestCreator webRequestCreator)
                            : base(args.RequestArgs, webRequestCreator)
        {
        }

        /// <summary>
        /// 获取响应头信息
        /// </summary>
        /// <param name="name">响应头名称（不区分大小写）</param>
        /// <returns>响应头的值，如果不存在或请求未完成则返回 null。</returns>
        public string GetResponseHeader(string name)
        {
            if (_cachedResponseHeaders == null)
                return null;

            if (_cachedResponseHeaders.TryGetValue(name, out string value))
                return value;

            return null;
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            var request = CreateHeadWebRequest(Url);
            request.downloadHandler = null; // HEAD 请求不需要 DownloadHandler
            return request;
        }

        protected override void OnRequestSucceeded(UnityWebRequest webRequest)
        {
            var headers = webRequest.GetResponseHeaders();
            if (headers != null)
            {
                // 注意：UnityWebRequest 的响应头 key 是小写的
                _cachedResponseHeaders = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in headers)
                {
                    string name = kvp.Key;
                    string value = kvp.Value;
                    _cachedResponseHeaders[name] = value;
                }
            }
        }
    }
}