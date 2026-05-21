
namespace YooAsset
{
    /// <summary>
    /// 加载清单的操作选项
    /// </summary>
    public readonly struct LoadPackageManifestOptions
    {
        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { get; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 创建加载清单选项实例
        /// </summary>
        /// <param name="packageVersion">包裹版本</param>
        /// <param name="timeout">超时时间（秒）</param>
        public LoadPackageManifestOptions(string packageVersion, int timeout)
        {
            PackageVersion = packageVersion;
            Timeout = timeout;
        }

        /// <summary>
        /// 转换为 FileSystem 的加载清单选项
        /// </summary>
        internal FSLoadPackageManifestOptions ConvertTo()
        {
            return new FSLoadPackageManifestOptions(PackageVersion, Timeout);
        }
    }
}