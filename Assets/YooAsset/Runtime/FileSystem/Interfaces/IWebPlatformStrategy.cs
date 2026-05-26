using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// Web 平台 AssetBundle 请求创建参数
    /// </summary>
    internal readonly struct WebAssetBundleRequestArgs
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 禁用 Unity 的网络缓存
        /// </summary>
        public bool DisableUnityWebCache { get; }

        /// <summary>
        /// AssetBundle 文件哈希（用于 UnityWebRequest 的缓存）
        /// </summary>
        public string FileHash { get; }

        /// <summary>
        /// Unity CRC 校验值
        /// </summary>
        public uint UnityCrc { get; }

        /// <summary>
        /// 创建 <see cref="WebAssetBundleRequestArgs"/> 实例
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="disableUnityWebCache">是否禁用 Unity 内置缓存</param>
        /// <param name="fileHash">文件哈希（启用缓存时必须提供）</param>
        /// <param name="unityCrc">Unity CRC 校验值</param>
        internal WebAssetBundleRequestArgs(string url, bool disableUnityWebCache, string fileHash, uint unityCrc)
        {
            Url = url;
            DisableUnityWebCache = disableUnityWebCache;
            FileHash = fileHash;
            UnityCrc = unityCrc;
        }
    }

    /// <summary>
    /// Web 平台策略接口
    /// </summary>
    internal interface IWebPlatformStrategy
    {
        /// <summary>
        /// 创建平台专用的 AssetBundle 下载请求
        /// </summary>
        /// <param name="args">AssetBundle 下载参数</param>
        /// <returns>已配置的 UnityWebRequest 实例</returns>
        UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args);

        /// <summary>
        /// 从已完成的请求中提取 AssetBundle 对象
        /// </summary>
        /// <param name="request">已完成下载的 UnityWebRequest</param>
        /// <returns>提取到的 AssetBundle，若提取失败则返回 null。</returns>
        AssetBundle ExtractAssetBundle(UnityWebRequest request);

        /// <summary>
        /// 使用平台专用 API 卸载 AssetBundle
        /// </summary>
        /// <param name="assetBundle">待卸载的 AssetBundle 实例</param>
        /// <param name="unloadAll">是否同时卸载所有已加载的资源对象</param>
        void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll);
    }
}
