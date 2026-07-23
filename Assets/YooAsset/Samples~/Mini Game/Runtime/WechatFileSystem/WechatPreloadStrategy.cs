#if UNITY_WEBGL && (WEIXINMINIGAME || UNITY_WECHATMINIGAME)
using UnityEngine.Networking;
using YooAsset;
using WeChatWASM;

/// <summary>
/// 微信小游戏预下载策略
/// </summary>
internal sealed class WechatPreloadStrategy : IWebPreloadStrategy
{
    private readonly UnityWebRequestCreator _webRequestCreator;

    public WechatPreloadStrategy(UnityWebRequestCreator webRequestCreator)
    {
        _webRequestCreator = webRequestCreator;
    }

    /// <inheritdoc />
    public bool IsBundleCached(WebPreloadQueryArgs args)
    {
        // 注意：WX.GetCachePath会产生严重的同步调用开销，因此不主动查询平台缓存。
        // 此处保守返回false，允许预下载请求交由微信缓存机制自行处理。

        /*
        foreach (string url in args.CandidateUrls)
        {
            if (string.IsNullOrEmpty(WX.GetCachePath(url)) == false)
                return true;
        }
         */

        return false;
    }

    /// <inheritdoc />
    public UnityWebRequest CreatePreloadRequest(WebPreloadRequestArgs args)
    {
        UnityWebRequest request;
        if (_webRequestCreator != null)
            request = _webRequestCreator.Invoke(args.Url, UnityWebRequest.kHttpVerbGET);
        else
            request = new UnityWebRequest(args.Url, UnityWebRequest.kHttpVerbGET);

        if (request == null)
            throw new YooInternalException($"{nameof(UnityWebRequestCreator)} returned null.");

        request.disposeDownloadHandlerOnDispose = true;
        request.SetRequestHeader("wechatminigame-preload", "1");
        return request;
    }
}
#endif
