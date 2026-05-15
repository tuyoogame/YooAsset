using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 资源包句柄接口，提供对已加载资源包的操作能力。
    /// </summary>
    internal interface IBundleHandle
    {
        /// <summary>
        /// 卸载资源包
        /// </summary>
        void UnloadBundle();

        /// <summary>
        /// 加载指定的主资源对象
        /// </summary>
        /// <param name="assetInfo">待加载资源的描述信息</param>
        /// <returns>用于跟踪加载过程的异步操作对象</returns>
        BHLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo);

        /// <summary>
        /// 加载资源包内的全部资源对象
        /// </summary>
        /// <param name="assetInfo">待加载资源的描述信息</param>
        /// <returns>用于跟踪加载过程的异步操作对象</returns>
        BHLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 加载指定资源对应的全部子资源对象
        /// </summary>
        /// <param name="assetInfo">待加载资源的描述信息</param>
        /// <returns>用于跟踪加载过程的异步操作对象</returns>
        BHLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 加载指定的场景资源
        /// </summary>
        /// <param name="assetInfo">待加载场景的资源信息</param>
        /// <param name="loadSceneParams">场景加载选项</param>
        /// <param name="allowSceneActivation">是否允许场景在加载完成后立即激活</param>
        /// <returns>用于跟踪场景加载过程的异步操作对象</returns>
        BHLoadSceneOperation LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool allowSceneActivation);
    }
}
