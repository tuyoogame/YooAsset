using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest AssetBundle 下载器
    /// </summary>
    internal sealed class UnityWebRequestAssetBundle : UnityWebRequestBase, IDownloadAssetBundleRequest
    {
        private readonly DownloadAssetBundleRequestArgs _args;
        private readonly IWebPlatformStrategy _platformStrategy;

        /// <summary>
        /// 下载结果（AssetBundle 对象）
        /// </summary>
        public AssetBundle Result { get; private set; }

        /// <summary>
        /// 构造 AssetBundle 下载器
        /// </summary>
        /// <param name="args">AssetBundle 下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestAssetBundle(DownloadAssetBundleRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.RequestArgs, webRequestCreator)
        {
            _args = args;
            _platformStrategy = args.PlatformStrategy;
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            var args = new WebAssetBundleRequestArgs(
                url: _args.RequestArgs.Url,
                disableUnityWebCache: _args.DisableUnityWebCache,
                fileHash: _args.FileHash,
                unityCrc: _args.UnityCrc);
            return _platformStrategy.CreateAssetBundleRequest(args);
        }

        protected override void OnRequestSucceeded(UnityWebRequest webRequest)
        {
            Result = _platformStrategy.ExtractAssetBundle(webRequest);
        }
    }
}