
namespace YooAsset
{
    /// <summary>
    /// 下载资源包的操作选项
    /// </summary>
    internal readonly struct FSDownloadBundleOptions
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        public PackageBundle Bundle { get; }

        /// <summary>
        /// 失败后重试次数
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// 待导入的本地文件路径
        /// </summary>
        public string ImportFilePath { get; }

        public FSDownloadBundleOptions(PackageBundle bundle, int retryCount)
        {
            Bundle = bundle;
            RetryCount = retryCount;
            ImportFilePath = null;
        }
        public FSDownloadBundleOptions(PackageBundle bundle, int retryCount, string importFilePath)
        {
            Bundle = bundle;
            RetryCount = retryCount;
            ImportFilePath = importFilePath;
        }
    }
}