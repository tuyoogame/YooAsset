using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 虚拟资源包句柄
    /// </summary>
    internal sealed class VirtualBundleHandle : IBundleHandle
    {
        private readonly string _bundleFilePath;
        private readonly PackageBundle _packageBundle;

        /// <inheritdoc/>
        public string BundleFilePath
        {
            get { return _bundleFilePath; }
        }

        /// <summary>
        /// 创建 VirtualBundleHandle 实例
        /// </summary>
        /// <param name="bundleFilePath">资源包文件的本地路径</param>
        /// <param name="packageBundle">资源包描述</param>
        public VirtualBundleHandle(string bundleFilePath, PackageBundle packageBundle)
        {
            _bundleFilePath = bundleFilePath;
            _packageBundle = packageBundle;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new VBHLoadAssetOperation(_packageBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new VBHLoadAllAssetsOperation(_packageBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new VBHLoadSubAssetsOperation(_packageBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new VBHLoadSceneOperation(assetInfo, loadSceneParams, allowSceneActivation);
            return operation;
        }
    }
}