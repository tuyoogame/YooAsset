
namespace YooAsset
{
    /// <summary>
    /// 预取清单的操作选项
    /// </summary>
    public readonly struct PrefetchManifestOptions
    {
        /// <summary>
        /// 预取的包裹版本
        /// </summary>
        public string PackageVersion { get; }

        /// <summary>
        /// 资源清单请求超时时间
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 创建预取清单选项实例
        /// </summary>
        /// <param name="packageVersion">预取的包裹版本</param>
        /// <param name="timeout">资源清单请求超时时间（秒）</param>
        public PrefetchManifestOptions(string packageVersion, int timeout)
        {
            PackageVersion = packageVersion;
            Timeout = timeout;
        }
    }
}