#if UNITY_WEBGL && VIVOMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

/// <summary>
/// vivo 小游戏平台实现
/// </summary>
internal class VivoPlatform : IWebPlatformStrategy
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
    {
        UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(args.Url);
        request.disposeDownloadHandlerOnDispose = true;
        return request;
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.Unload(unloadAll);
    }
}
#endif
