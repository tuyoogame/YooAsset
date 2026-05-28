
namespace YooAsset
{
    /// <summary>
    /// 本地加载 RawBundle 的操作选项
    /// </summary>
    internal readonly struct LoadLocalRawBundleOptions
    {
        /// <summary>
        /// 文件缓存名称
        /// </summary>
        public string CacheName { get; }

        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 文件加载路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// RawBundle 解密器
        /// </summary>
        public IBundleDecryptor RawBundleDecryptor { get; }

        public LoadLocalRawBundleOptions(string cacheName, PackageBundle bundle, string filePath, IBundleDecryptor rawBundleDecryptor)
        {
            CacheName = cacheName;
            Bundle = bundle;
            FilePath = filePath;
            RawBundleDecryptor = rawBundleDecryptor;
        }
    }
}
