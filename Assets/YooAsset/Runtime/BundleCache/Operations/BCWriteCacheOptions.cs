
namespace YooAsset
{
    /// <summary>
    /// 写入缓存的操作选项
    /// </summary>
    internal readonly struct BCWriteCacheOptions
    {
        /// <summary>
        /// 要缓存的资源包
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 要缓存的文件路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 要缓存的文件数据（可选）
        /// </summary>
        public byte[] FileData { get; }

        public BCWriteCacheOptions(PackageBundle bundle, string filePath, byte[] fileData = null)
        {
            Bundle = bundle;
            FilePath = filePath;
            FileData = fileData;
        }
    }
}
