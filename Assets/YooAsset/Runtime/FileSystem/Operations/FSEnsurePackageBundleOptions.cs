
namespace YooAsset
{
    /// <summary>
    /// 确保资源包已就绪的操作选项
    /// </summary>
    internal readonly struct FSEnsurePackageBundleOptions
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        public FSEnsurePackageBundleOptions(PackageBundle bundle)
        {
            Bundle = bundle;
        }
    }
}
