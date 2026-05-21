
namespace YooAsset
{
    /// <summary>
    /// 加载资源包的操作选项
    /// </summary>
    internal readonly struct FSLoadPackageBundleOptions
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        public FSLoadPackageBundleOptions(PackageBundle bundle)
        {
            Bundle = bundle;
        }

        /// <summary>
        /// 转换为 BundleCache 的加载选项
        /// </summary>
        public BCLoadBundleOptions ConvertTo()
        {
            return new BCLoadBundleOptions(Bundle);
        }
    }
}
