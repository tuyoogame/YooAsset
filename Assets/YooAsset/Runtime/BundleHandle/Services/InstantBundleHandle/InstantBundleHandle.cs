#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// InstantAsset 资源包句柄
    /// </summary>
    internal sealed class InstantBundleHandle : IBundleHandle
    {
        private readonly PackageBundle _packageBundle;
        private readonly InstantAssetTable _assetTable;
        private readonly InstantAssetTable _sceneTable;

        public InstantBundleHandle(PackageBundle packageBundle, InstantAssetTable assetTable, InstantAssetTable sceneTable)
        {
            _packageBundle = packageBundle;
            _assetTable = assetTable;
            _sceneTable = sceneTable;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new IBHLoadAssetOperation(_packageBundle, _assetTable, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new IBHLoadAllAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new IBHLoadSubAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new IBHLoadSceneOperation(_sceneTable, assetInfo, loadSceneParams, allowSceneActivation);
            return operation;
        }
    }
}
#endif
