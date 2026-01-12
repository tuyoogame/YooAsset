using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 原生资源包句柄
    /// </summary>
    internal sealed class RawBundleHandle : IBundleHandle
    {
        private readonly string _bundleFilePath;
        private readonly PackageBundle _packageBundle;
        private readonly RawBundle _rawBundle;

        /// <inheritdoc/>
        public string BundleFilePath
        {
            get { return _bundleFilePath; }
        }

        /// <summary>
        /// 创建 RawBundleHandle 实例
        /// </summary>
        /// <param name="bundleFilePath">资源包文件的本地路径</param>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="rawBundle">已加载的原生资源包数据对象</param>
        public RawBundleHandle(string bundleFilePath, PackageBundle packageBundle, RawBundle rawBundle)
        {
            _bundleFilePath = bundleFilePath;
            _packageBundle = packageBundle;
            _rawBundle = rawBundle;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
            if (_rawBundle != null)
            {
                _rawBundle.Unload();
            }
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new RBHLoadAssetOperation(_packageBundle, _rawBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new RBHLoadAllAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new RBHLoadSubAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new RBHLoadSceneOperation();
            return operation;
        }
    }
}