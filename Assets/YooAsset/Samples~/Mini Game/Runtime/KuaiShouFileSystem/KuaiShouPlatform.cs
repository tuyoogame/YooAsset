#if UNITY_WEBGL && KUAISHOUMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using KSWASM;

/// <summary>
/// 快手小游戏平台实现
/// </summary>
internal class KuaiShouPlatform : IWebGamePlatform
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return KSAssetBundle.GetAssetBundle(url);
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerKSAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.KSUnload(unloadAll);
    }
}
#endif
