#if UNITY_WEBGL && WEIXINMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using WeChatWASM;

/// <summary>
/// 微信小游戏平台实现
/// </summary>
internal class WechatPlatform : IWebGamePlatform
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return WXAssetBundle.GetAssetBundle(url);
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerWXAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.WXUnload(unloadAll);
    }

    /// <inheritdoc/>
    public bool IsCached(string cacheFilePath)
    {
        if (string.IsNullOrEmpty(cacheFilePath))
            return false;

        string result = WX.GetCachePath(cacheFilePath);
        return string.IsNullOrEmpty(result) == false;
    }

    /// <inheritdoc/>
    public string GetCacheFilePath(string rootPath, PackageBundle bundle)
    {
        return PathUtility.Combine(rootPath, bundle.GetFileName());
    }
}
#endif
