
namespace YooAsset
{
    /// <summary>
    /// 资源包解包判定信息
    /// </summary>
    public readonly struct BundleUnpackInfo
    {
        private readonly PackageBundle _bundle;

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName => _bundle.BundleName;

        /// <summary>
        /// 资源包文件名称
        /// </summary>
        public string FileName => _bundle.GetFileName();

        /// <summary>
        /// 资源包类型
        /// </summary>
        public int BundleType => _bundle.GetBundleType();

        /// <summary>
        /// 是否为加密资源包
        /// </summary>
        public bool IsEncrypted => _bundle.IsEncrypted;

        /// <summary>
        /// 分类标签集合
        /// </summary>
        public PackageTags Tags => _bundle.Tags;

        internal BundleUnpackInfo(PackageBundle bundle)
        {
            _bundle = bundle;
        }
    }

    /// <summary>
    /// 内置资源包解包策略接口
    /// </summary>
    public interface IBundleUnpackPolicy
    {
        /// <summary>
        /// 判定指定资源包是否为需要解包的类型
        /// </summary>
        bool IsUnpackBundle(BundleUnpackInfo unpackInfo);
    }
}
