using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台 AssetBundle 下载器
    /// </summary>
    /// <remarks>
    /// 创建平台专用请求并提取 AssetBundle 对象
    /// </remarks>
    internal sealed class WebGameAssetBundleRequest : UnityWebRequestBase, IDownloadAssetBundleRequest
    {
        private readonly IWebGamePlatform _platform;

        /// <summary>
        /// 下载结果（AssetBundle 对象）
        /// </summary>
        public AssetBundle Result { get; private set; }

        /// <summary>
        /// 构造 AssetBundle 下载器
        /// </summary>
        /// <param name="requestArgs">公共请求参数</param>
        /// <param name="platform">游戏平台接口</param>
        public WebGameAssetBundleRequest(DownloadRequestArgs requestArgs, IWebGamePlatform platform)
            : base(requestArgs, null)
        {
            _platform = platform;
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            var request = _platform.CreateAssetBundleRequest(Url);
            request.disposeDownloadHandlerOnDispose = true;
            return request;
        }

        protected override void OnRequestSucceeded(UnityWebRequest webRequest)
        {
            Result = _platform.ExtractAssetBundle(webRequest);
        }
    }
}
