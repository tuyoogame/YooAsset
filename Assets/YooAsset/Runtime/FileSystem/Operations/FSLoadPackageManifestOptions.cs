
namespace YooAsset
{
    /// <summary>
    /// 加载包裹清单的操作选项
    /// </summary>
    internal readonly struct FSLoadPackageManifestOptions
    {
        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { get; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; }

        public FSLoadPackageManifestOptions(string packageVersion, int timeout)
        {
            PackageVersion = packageVersion;
            Timeout = timeout;
        }
    }
}
