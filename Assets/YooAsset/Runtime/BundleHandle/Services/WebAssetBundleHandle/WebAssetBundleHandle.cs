using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// WebGL 平台 AssetBundle 资源包句柄
    /// </summary>
    internal sealed class WebAssetBundleHandle : IBundleHandle
    {
        private readonly PackageBundle _packageBundle;
        private readonly AssetBundle _assetBundle;
        private readonly IWebPlatformStrategy _platformStrategy;

        public WebAssetBundleHandle(PackageBundle packageBundle, AssetBundle assetBundle, IWebPlatformStrategy platformStrategy)
        {
            _packageBundle = packageBundle;
            _assetBundle = assetBundle;
            _platformStrategy = platformStrategy;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
            if (_assetBundle != null)
            {
                if (_packageBundle.IsEncrypted)
                    _assetBundle.Unload(true);
                else
                    _platformStrategy.UnloadAssetBundle(_assetBundle, true);
            }
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new ABHLoadAssetOperation(_packageBundle, _assetBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new ABHLoadAllAssetsOperation(_packageBundle, _assetBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new ABHLoadSubAssetsOperation(_packageBundle, _assetBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new ABHLoadSceneOperation(assetInfo, loadSceneParams, allowSceneActivation);
            return operation;
        }
    }
}
