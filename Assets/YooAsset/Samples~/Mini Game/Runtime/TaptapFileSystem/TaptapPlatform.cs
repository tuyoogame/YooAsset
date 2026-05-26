#if UNITY_WEBGL && TAPMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using TapTapMiniGame;

/// <summary>
/// TapTap 小游戏平台实现
/// 参考：https://developer.taptap.cn/minigameapidoc/dev/engine/unity-adaptation/guide/
/// </summary>
internal class TaptapPlatform : IWebPlatformStrategy
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
    {
        UnityWebRequest request = TapAssetBundle.GetAssetBundle(args.Url);
        request.disposeDownloadHandlerOnDispose = true;
        return request;
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
}
#endif
