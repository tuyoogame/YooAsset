#if UNITY_WEBGL && KUAISHOUMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using KSWASM;

/// <summary>
/// 快手小游戏平台实现
/// </summary>
internal class KuaiShouPlatform : IWebPlatformStrategy
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
    {
        UnityWebRequest request = KSAssetBundle.GetAssetBundle(args.Url);
        request.disposeDownloadHandlerOnDispose = true;
        return request;
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
