
namespace YooAsset
{
    /// <summary>
    /// 卸载所有资源的操作选项
    /// </summary>
    public readonly struct UnloadAllAssetsOptions
    {
        /// <summary>
        /// 是否释放所有资源句柄，防止卸载过程中触发完成回调。
        /// </summary>
        public bool ShouldReleaseHandles { get; }

        /// <summary>
        /// 是否在卸载过程中锁定加载操作，防止新的任务请求。
        /// </summary>
        public bool ShouldLockLoading { get; }

        /// <summary>
        /// 是否等待引擎底层资源卸载完毕
        /// </summary>
        /// <remarks>
        /// 开启后会等待引擎底层资源卸载完毕，该过程耗时较长会显著拉长本操作的等待时间。
        /// </remarks>
        public bool ShouldWaitUnloadUnused { get; }

        /// <summary>
        /// 创建卸载所有资源的选项
        /// </summary>
        /// <param name="shouldReleaseHandles">是否释放所有句柄</param>
        /// <param name="shouldLockLoading">是否锁定加载操作</param>
        public UnloadAllAssetsOptions(bool shouldReleaseHandles, bool shouldLockLoading)
            : this(shouldReleaseHandles, shouldLockLoading, false)
        {
        }

        /// <summary>
        /// 创建卸载所有资源的选项
        /// </summary>
        /// <param name="shouldReleaseHandles">是否释放所有句柄</param>
        /// <param name="shouldLockLoading">是否锁定加载操作</param>
        /// <param name="shouldWaitUnloadUnused">是否等待底层资源卸载完毕</param>
        public UnloadAllAssetsOptions(bool shouldReleaseHandles, bool shouldLockLoading, bool shouldWaitUnloadUnused)
        {
            ShouldReleaseHandles = shouldReleaseHandles;
            ShouldLockLoading = shouldLockLoading;
            ShouldWaitUnloadUnused = shouldWaitUnloadUnused;
        }
    }
}