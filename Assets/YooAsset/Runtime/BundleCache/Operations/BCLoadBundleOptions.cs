
namespace YooAsset
{
    /// <summary>
    /// 加载资源包的操作选项
    /// </summary>
    internal readonly struct BCLoadBundleOptions
    {
        /// <summary>
        /// 要加载的资源包
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 创建加载资源包操作选项实例
        /// </summary>
        /// <param name="bundle">要加载的资源包描述</param>
        public BCLoadBundleOptions(PackageBundle bundle)
        {
            Bundle = bundle;
        }
    }
}