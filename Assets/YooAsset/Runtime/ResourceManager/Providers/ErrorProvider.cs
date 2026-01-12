
namespace YooAsset
{
    /// <summary>
    /// 错误提供者，用于处理加载失败的情况。
    /// </summary>
    internal sealed class ErrorProvider : ProviderBase
    {
        /// <summary>
        /// 创建错误提供者实例
        /// </summary>
        /// <param name="manager">资源管理器</param>
        /// <param name="assetInfo">资源信息</param>
        public ErrorProvider(ResourceManager manager, AssetInfo assetInfo) : base(manager, string.Empty, assetInfo)
        {
        }

        protected override void InternalProcessBundleHandle()
        {
        }

        /// <summary>
        /// 设置错误完成状态
        /// </summary>
        /// <param name="error">错误信息</param>
        public void SetCompletedWithError(string error)
        {
            SetFail(error);
        }
    }
}