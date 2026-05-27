
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
        /// 分类标签数量
        /// </summary>
        public int TagCount
        {
            get
            {
                return _bundle.Tags == null ? 0 : _bundle.Tags.Length;
            }
        }

        internal BundleUnpackInfo(PackageBundle bundle)
        {
            _bundle = bundle;
        }

        /// <summary>
        /// 获取指定索引的分类标签
        /// </summary>
        public string GetTag(int index)
        {
            return _bundle.Tags[index];
        }

        /// <summary>
        /// 是否包含指定的单个标签
        /// </summary>
        public bool HasTag(string tag)
        {
            return _bundle.HasTag(tag);
        }

        /// <summary>
        /// 是否包含指定标签数组中的任意一个
        /// </summary>
        public bool HasAnyTag(string[] tags)
        {
            return _bundle.HasAnyTag(tags);
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
