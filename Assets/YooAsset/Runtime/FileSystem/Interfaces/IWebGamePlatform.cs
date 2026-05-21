using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台接口
    /// </summary>
    internal interface IWebGamePlatform
    {
        /// <summary>
        /// 创建平台专用的 AssetBundle 下载请求
        /// </summary>
        /// <param name="url">资源包的远程地址</param>
        /// <returns>已配置的 UnityWebRequest 实例</returns>
        UnityWebRequest CreateAssetBundleRequest(string url);

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
