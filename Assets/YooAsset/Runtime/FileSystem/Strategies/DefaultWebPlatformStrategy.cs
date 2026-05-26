using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 网页端平台策略的默认实现
    /// </summary>
    internal sealed class DefaultWebPlatformStrategy : IWebPlatformStrategy
    {
        private readonly UnityWebRequestCreator _webRequestCreator;

        public DefaultWebPlatformStrategy(UnityWebRequestCreator webRequestCreator)
        {
            _webRequestCreator = webRequestCreator;
        }

        /// <inheritdoc/>
        public UnityWebRequest CreateAssetBundleRequest(WebAssetBundleRequestArgs args)
        {
            var downloadHandler = CreateAssetBundleDownloadHandler(args);
            var request = CreateGetWebRequest(args.Url);
            request.downloadHandler = downloadHandler;
            request.disposeDownloadHandlerOnDispose = true;
            return request;
        }

        /// <inheritdoc/>
        public AssetBundle ExtractAssetBundle(UnityWebRequest request)
        {
            var handler = (DownloadHandlerAssetBundle)request.downloadHandler;
            return handler.assetBundle;
        }

        /// <inheritdoc/>
        public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
        {
            assetBundle.Unload(unloadAll);
        }

        private UnityWebRequest CreateGetWebRequest(string url)
        {
            if (_webRequestCreator != null)
                return _webRequestCreator.Invoke(url, UnityWebRequest.kHttpVerbGET);

            return new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        }
        private DownloadHandlerAssetBundle CreateAssetBundleDownloadHandler(WebAssetBundleRequestArgs args)
        {
            if (args.DisableUnityWebCache)
            {
                // 禁用 Unity 缓存
                return new DownloadHandlerAssetBundle(args.Url, args.UnityCrc);
            }
            else
            {
                if (string.IsNullOrEmpty(args.FileHash))
                    throw new YooInternalException("FileHash is required when Unity web cache is enabled (DisableUnityWebCache = false).");

                // 使用 Unity 缓存
                // 说明：The file hash defining the version of the asset bundle.
                Hash128 fileHash = Hash128.Parse(args.FileHash);
                return new DownloadHandlerAssetBundle(args.Url, fileHash, args.UnityCrc);
            }
        }
    }
}
