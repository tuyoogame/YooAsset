
namespace YooAsset
{
    /// <summary>
    /// 本地加载 ArchiveBundle 的操作选项
    /// </summary>
    internal readonly struct LoadLocalArchiveBundleOptions
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
        /// ArchiveBundle 解密器
        /// </summary>
        public IBundleDecryptor ArchiveBundleDecryptor { get; }

        public LoadLocalArchiveBundleOptions(string cacheName, PackageBundle bundle, string filePath, IBundleDecryptor archiveBundleDecryptor)
        {
            CacheName = cacheName;
            Bundle = bundle;
            FilePath = filePath;
            ArchiveBundleDecryptor = archiveBundleDecryptor;
        }
    }
}
