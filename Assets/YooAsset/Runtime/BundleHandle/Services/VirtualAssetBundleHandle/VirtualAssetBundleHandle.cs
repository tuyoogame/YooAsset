using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal sealed class VirtualAssetBundleHandle : IBundleHandle
    {
        private readonly PackageBundle _packageBundle;

        /// <summary>
        /// 创建 VirtualAssetBundleHandle 实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        public VirtualAssetBundleHandle(PackageBundle packageBundle)
        {
            _packageBundle = packageBundle;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new VABHLoadAssetOperation(_packageBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new VABHLoadAllAssetsOperation(_packageBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new VABHLoadSubAssetsOperation(_packageBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new VABHLoadSceneOperation(assetInfo, loadSceneParams, allowSceneActivation);
            return operation;
        }
    }
}