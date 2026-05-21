#if UNITY_WEBGL && OPPOMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

/// <summary>
/// OPPO 小游戏平台实现
/// </summary>
internal class OppoPlatform : IWebGamePlatform
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url);
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
