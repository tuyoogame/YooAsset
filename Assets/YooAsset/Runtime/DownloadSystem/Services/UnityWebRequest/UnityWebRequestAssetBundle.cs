using System;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// UnityWebRequest AssetBundle 下载器
    /// </summary>
    /// <remarks>
    /// <para>下载并加载 Unity AssetBundle 资源包</para>
    /// <para>支持 Unity 内置缓存机制和 CRC 校验</para>
    /// </remarks>
    internal sealed class UnityWebRequestAssetBundle : UnityWebRequestBase, IDownloadAssetBundleRequest
    {
        /// <summary>
        /// AssetBundle 下载参数
        /// </summary>
        private readonly DownloadAssetBundleRequestArgs _args;

        /// <summary>
        /// AssetBundle 下载处理器
        /// </summary>
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
        public UnityWebRequestAssetBundle(DownloadAssetBundleRequestArgs args, UnityWebRequestCreator webRequestCreator)
            : base(args.RequestArgs, webRequestCreator)
        {
            _args = args;
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            _downloadHandler = CreateAssetBundleDownloadHandler();
            var request = CreateGetWebRequest(Url);
            request.downloadHandler = _downloadHandler;
            request.disposeDownloadHandlerOnDispose = true;
            return request;
        }

        protected override void OnRequestSucceeded(UnityWebRequest webRequest)
        {
            Result = _downloadHandler.assetBundle;
        }

        private DownloadHandlerAssetBundle CreateAssetBundleDownloadHandler()
        {
            DownloadHandlerAssetBundle handler;

            if (_args.DisableUnityWebCache)
            {
                // 禁用 Unity 缓存
                handler = new DownloadHandlerAssetBundle(Url, _args.UnityCrc);
            }
            else
            {
                if (string.IsNullOrEmpty(_args.FileHash))
                    throw new YooInternalException("FileHash is required when Unity web cache is enabled (DisableUnityWebCache = false).");

                // 使用 Unity 缓存
                // 说明：The file hash defining the version of the asset bundle.
                Hash128 fileHash = Hash128.Parse(_args.FileHash);
                handler = new DownloadHandlerAssetBundle(Url, fileHash, _args.UnityCrc);
            }

            return handler;
        }
    }
}