using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 归档文件资源包句柄
    /// </summary>
    internal sealed class ArchiveBundleHandle : IBundleHandle
    {
        private readonly PackageBundle _packageBundle;
        private readonly ArchiveBundle _archiveBundle;

        /// <summary>
        /// 创建 ArchiveBundleHandle 实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="archiveBundle">已解析的归档资源包数据对象</param>
        public ArchiveBundleHandle(PackageBundle packageBundle, ArchiveBundle archiveBundle)
        {
            _packageBundle = packageBundle;
            _archiveBundle = archiveBundle;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
            if (_archiveBundle != null)
            {
                _archiveBundle.Unload();
            }
        }

        /// <inheritdoc/>
        public BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new ARBHLoadAssetOperation(_packageBundle, _archiveBundle, assetInfo);
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new ARBHLoadAllAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new ARBHLoadSubAssetsOperation();
            return operation;
        }

        /// <inheritdoc/>
        public BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation)
        {
            var operation = new ARBHLoadSceneOperation();
            return operation;
        }
    }
}
