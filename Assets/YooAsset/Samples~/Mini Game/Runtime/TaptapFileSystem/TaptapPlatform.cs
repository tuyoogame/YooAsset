#if UNITY_WEBGL && TAPMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using TapTapMiniGame;

/// <summary>
/// TapTap 小游戏平台实现
/// 参考：https://developer.taptap.cn/minigameapidoc/dev/engine/unity-adaptation/guide/
/// </summary>
internal class TaptapPlatform : IWebGamePlatform
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return TapAssetBundle.GetAssetBundle(url);
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerTapAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.TapUnload(unloadAll);
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
