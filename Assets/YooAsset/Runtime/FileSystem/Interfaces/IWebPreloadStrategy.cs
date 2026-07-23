using System.Collections.Generic;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// Web 平台预下载缓存查询参数
    /// </summary>
    internal readonly struct WebPreloadQueryArgs
    {
        /// <summary>
        /// 目标资源包
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 候选下载地址
        /// </summary>
        public IReadOnlyList<string> CandidateUrls { get; }

        internal WebPreloadQueryArgs(PackageBundle bundle, IReadOnlyList<string> candidateUrls)
        {
            Bundle = bundle;
            CandidateUrls = candidateUrls;
        }
    }

    /// <summary>
    /// Web 平台预下载请求创建参数
    /// </summary>
    internal readonly struct WebPreloadRequestArgs
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; }

        internal WebPreloadRequestArgs(string url)
        {
            Url = url;
        }
    }

    /// <summary>
    /// Web 平台预下载策略接口
    /// </summary>
    internal interface IWebPreloadStrategy
    {
        /// <summary>
        /// 查询指定资源包是否已经存在于平台缓存
        /// </summary>
        /// <param name="args">预下载缓存查询参数</param>
        /// <returns>资源已经缓存时返回 true，否则返回 false。</returns>
        bool IsBundleCached(WebPreloadQueryArgs args);

        /// <summary>
        /// 创建平台专用的预下载请求
        /// </summary>
        /// <param name="args">预下载请求参数</param>
        /// <returns>已配置但尚未发送的 UnityWebRequest 实例</returns>
        UnityWebRequest CreatePreloadRequest(WebPreloadRequestArgs args);
    }
}
