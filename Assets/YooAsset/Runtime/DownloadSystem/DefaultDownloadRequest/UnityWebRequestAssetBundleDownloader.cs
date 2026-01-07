using System;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest AssetBundle 下载器
    /// </summary>
    /// <remarks>
    /// 下载并加载 Unity AssetBundle 资源包。
    /// 支持 Unity 内置缓存机制和 CRC 校验。
    /// </remarks>
    internal sealed class UnityWebRequestAssetBundleDownloader : UnityWebRequestDownloaderBase, IDownloadAssetBundleRequest
    {
        private readonly DownloadAssetBundleRequestArgs _args;
        private DownloadHandlerAssetBundle _downloadHandler;

        /// <summary>
        /// 下载结果（AssetBundle 对象）
        /// </summary>
        public AssetBundle Result { get; private set; }

        /// <summary>
        /// 构造 AssetBundle 下载器
        /// </summary>
        /// <param name="args">AssetBundle 下载参数</param>
        /// <param name="webRequestCreator">UnityWebRequest 创建器（可选）</param>
        public UnityWebRequestAssetBundleDownloader(DownloadAssetBundleRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.URL, webRequestCreator)
        {
            _args = args;
        }

        /// <summary>
        /// 创建 UnityWebRequest
        /// </summary>
        protected override void CreateWebRequest()
        {
            _downloadHandler = CreateAssetBundleDownloadHandler();
            _webRequest = CreateUnityWebRequestGet(URL);
            _webRequest.downloadHandler = _downloadHandler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            ApplyRequestOptions(_args.Timeout, _args.WatchdogTime, _args.Headers);
        }

        /// <summary>
        /// 请求成功时的回调
        /// </summary>
        protected override void OnRequestSucceed()
        {
            AssetBundle assetBundle = _downloadHandler.assetBundle;
            if (assetBundle == null)
            {
                Status = EDownloadRequestStatus.Failed;
                Error = $"[{GetType().Name}] URL: {URL} - AssetBundle object is null";
            }
            else
            {
                Result = assetBundle;
            }
        }

        /// <summary>
        /// 创建 AssetBundle 下载处理器
        /// </summary>
        private DownloadHandlerAssetBundle CreateAssetBundleDownloadHandler()
        {
            DownloadHandlerAssetBundle handler;

            if (_args.DisableUnityWebCache)
            {
                // 禁用 Unity 缓存
                handler = new DownloadHandlerAssetBundle(URL, _args.UnityCRC);
            }
            else
            {
                if (string.IsNullOrEmpty(_args.FileHash))
                    throw new YooInternalException("File hash is null or empty !");

                // 使用 Unity 缓存
                // 说明：The file hash defining the version of the asset bundle.
                Hash128 fileHash = Hash128.Parse(_args.FileHash);
                handler = new DownloadHandlerAssetBundle(URL, fileHash, _args.UnityCRC);
            }

            return handler;
        }
    }
}
