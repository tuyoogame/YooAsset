#if UNITY_WEBGL && (WEIXINMINIGAME || UNITY_WECHATMINIGAME)
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using WeChatWASM;

/// <summary>
/// 微信小游戏平台实现
/// </summary>
internal class WechatPlatform : IWebPlatformStrategy
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
    {
        UnityWebRequest request = WXAssetBundle.GetAssetBundle(args.Url);
        request.disposeDownloadHandlerOnDispose = true;
        return request;
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
}
#endif
