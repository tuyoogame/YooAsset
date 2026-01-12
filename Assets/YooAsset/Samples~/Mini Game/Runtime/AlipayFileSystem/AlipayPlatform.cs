#if UNITY_WEBGL && UNITY_ALIMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using AlipaySdk;

/// <summary>
/// 支付宝小游戏平台实现
/// 参考：https://opendocs.alipay.com/mini-game/
/// </summary>
internal class AlipayPlatform : IWebGamePlatform
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return APAssetBundle.GetAssetBundle(url);
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerAPAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.APUnload(unloadAll);
    }

    /// <inheritdoc/>
    public bool IsCached(string cacheFilePath)
    {
        return false;
    }

    /// <inheritdoc/>
    public string GetCacheFilePath(string rootPath, PackageBundle bundle)
    {
        return PathUtility.Combine(rootPath, bundle.GetFileName());
    }
}
#endif
