using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// WebGL 游戏平台 AssetBundle 资源包句柄
    /// </summary>
    internal sealed class WebGameAssetBundleHandle : IBundleHandle
    {
        private readonly string _bundleFilePath;
        private readonly PackageBundle _packageBundle;
        private readonly AssetBundle _assetBundle;
        private readonly IWebGamePlatform _platform;

        /// <inheritdoc/>
        public string BundleFilePath
        {
            get { return _bundleFilePath; }
        }

        /// <summary>
        /// 创建 WebGameAssetBundleHandle 实例
        /// </summary>
        /// <param name="bundleFilePath">资源包文件的本地路径</param>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="assetBundle">已加载的 AssetBundle 对象</param>
        /// <param name="platform">平台实现</param>
        public WebGameAssetBundleHandle(string bundleFilePath, PackageBundle packageBundle, AssetBundle assetBundle, IWebGamePlatform platform)
        {
            _bundleFilePath = bundleFilePath;
            _packageBundle = packageBundle;
            _assetBundle = assetBundle;
            _platform = platform;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
            if (_assetBundle != null)
            {
                if (_packageBundle.IsEncrypted)
                    _assetBundle.Unload(true);
                else
                    _platform.UnloadAssetBundle(_assetBundle, true);
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
