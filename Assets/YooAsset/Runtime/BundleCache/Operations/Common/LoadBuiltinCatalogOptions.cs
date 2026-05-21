
namespace YooAsset
{
    /// <summary>
    /// 加载内置资源目录的操作选项
    /// </summary>
    internal readonly struct LoadBuiltinCatalogOptions
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 下载后台
        /// </summary>
        public IDownloadBackend DownloadBackend { get; }

        public LoadBuiltinCatalogOptions(string packageName, string filePath, IDownloadBackend downloadBackend)
        {
            PackageName = packageName;
            FilePath = filePath;
            DownloadBackend = downloadBackend;
        }
    }
}