
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
        /// 创建卸载所有资源的选项
        /// </summary>
        /// <param name="shouldReleaseHandles">是否释放所有句柄</param>
        /// <param name="shouldLockLoading">是否锁定加载操作</param>
        public UnloadAllAssetsOptions(bool shouldReleaseHandles, bool shouldLockLoading)
        {
            ShouldReleaseHandles = shouldReleaseHandles;
            ShouldLockLoading = shouldLockLoading;
        }
    }
}