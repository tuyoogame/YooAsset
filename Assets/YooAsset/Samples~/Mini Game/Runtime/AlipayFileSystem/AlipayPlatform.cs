#if UNITY_WEBGL && UNITY_ALIMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using AlipaySdk;

/// <summary>
/// 支付宝小游戏平台实现
/// 参考：https://opendocs.alipay.com/mini-game/
/// </summary>
internal class AlipayPlatform : IWebPlatformStrategy
{
    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
    {
        UnityWebRequest request = APAssetBundle.GetAssetBundle(args.Url);
        request.disposeDownloadHandlerOnDispose = true;
        return request;
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
}
#endif
