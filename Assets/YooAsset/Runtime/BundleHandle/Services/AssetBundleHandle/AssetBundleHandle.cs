using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// AssetBundle 资源包句柄
    /// </summary>
    internal sealed class AssetBundleHandle : IBundleHandle
    {
        private readonly PackageBundle _packageBundle;
        private readonly AssetBundle _assetBundle;
        private readonly Stream _bundleStream;

        /// <summary>
        /// 创建 AssetBundleHandle 实例
        /// </summary>
        /// <param name="packageBundle">资源包描述</param>
        /// <param name="assetBundle">已加载的 AssetBundle 对象</param>
        /// <param name="bundleStream">加载 AssetBundle 时使用的文件流</param>
        public AssetBundleHandle(PackageBundle packageBundle, AssetBundle assetBundle, Stream bundleStream)
        {
            _packageBundle = packageBundle;
            _assetBundle = assetBundle;
            _bundleStream = bundleStream;
        }

        /// <inheritdoc/>
        public void UnloadBundle()
        {
            if (_assetBundle != null)
            {
                _assetBundle.Unload(true);
            }

            if (_bundleStream != null)
            {
                _bundleStream.Dispose();
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