using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 虚拟归档资源包句柄
    /// </summary>
    internal sealed class VirtualArchiveBundleHandle : IBundleHandle
    {
        private readonly string _bundleFilePath;
        private readonly PackageBundle _packageBundle;
        private readonly VirtualArchiveBundle _virtualArchiveBundle;

        /// <inheritdoc/>
        public string BundleFilePath
        {
            get { return _bundleFilePath; }
        }

        /// <summary>
        /// 创建 VirtualArchiveBundleHandle 实例
        /// </summary>
        /// <param name="bundleFilePath">用于调试显示的路径</param>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="virtualArchiveBundle">虚拟归档资源包数据对象</param>
        public VirtualArchiveBundleHandle(string bundleFilePath, PackageBundle packageBundle, VirtualArchiveBundle virtualArchiveBundle)
        {
            _bundleFilePath = bundleFilePath;
            _packageBundle = packageBundle;
            _virtualArchiveBundle = virtualArchiveBundle;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
            if (_virtualArchiveBundle != null)
            {
                _virtualArchiveBundle.Unload();
            }
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new VARBHLoadAssetOperation(_packageBundle, _virtualArchiveBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new VARBHLoadAllAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new VARBHLoadSubAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new VARBHLoadSceneOperation();
            return operation;
        }
    }
}
