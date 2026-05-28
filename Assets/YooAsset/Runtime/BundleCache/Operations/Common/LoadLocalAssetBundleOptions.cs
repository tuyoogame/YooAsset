
namespace YooAsset
{
    /// <summary>
    /// 本地加载 AssetBundle 的操作选项
    /// </summary>
    internal readonly struct LoadLocalAssetBundleOptions
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
        /// AssetBundle 解密器
        /// </summary>
        public IBundleDecryptor AssetBundleDecryptor { get; }

        public LoadLocalAssetBundleOptions(string cacheName, PackageBundle bundle, string filePath, IBundleDecryptor assetBundleDecryptor)
        {
            CacheName = cacheName;
            Bundle = bundle;
            FilePath = filePath;
            AssetBundleDecryptor = assetBundleDecryptor;
        }
    }
}

