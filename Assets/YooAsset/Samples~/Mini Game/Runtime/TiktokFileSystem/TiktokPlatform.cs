#if UNITY_WEBGL && DOUYINMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using TTSDK;

/// <summary>
/// 抖音小游戏平台实现
/// </summary>
internal class TiktokPlatform : IWebGamePlatform
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return TTAssetBundle.GetAssetBundle(url);
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerTTAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.TTUnload(unloadAll);
    }
}
#endif
